from fastapi import FastAPI
from pydantic import BaseModel
from transformers import BertTokenizer, BertForSequenceClassification, pipeline
from deep_translator import GoogleTranslator
from newsapi import NewsApiClient
from dotenv import load_dotenv
import os
import uvicorn

load_dotenv()

app = FastAPI()

NEWS_API_KEY = os.getenv("NEWS_API_KEY")
newsapi = NewsApiClient(api_key=NEWS_API_KEY)

print("FinBERT yukleniyor...")
finbert = BertForSequenceClassification.from_pretrained("yiyanghkust/finbert-tone", num_labels=3)
tokenizer = BertTokenizer.from_pretrained("yiyanghkust/finbert-tone")
nlp = pipeline("sentiment-analysis", model=finbert, tokenizer=tokenizer)
print("FinBERT hazir.")

translator = GoogleTranslator(source="tr", target="en")


class SentimentRequest(BaseModel):
    texts: list[str]
    il: str = ""


class SentimentResult(BaseModel):
    original_text: str
    translated_text: str
    label: str
    score: float


class SentimentResponse(BaseModel):
    il: str
    results: list[SentimentResult]
    sentiment_score: float
    haber_sayisi: int


def analyze_texts(il, texts):
    results = []
    score_sum = 0.0
    total = len(texts)
    for text in texts:
        try:
            translated = translator.translate(text[:500])
        except Exception:
            translated = text
        output = nlp(translated[:512])[0]
        label = output["label"]
        score = output["score"]
        if label == "Positive":
            score_sum += score
        elif label == "Negative":
            score_sum -= score
        results.append(
            SentimentResult(
                original_text=text,
                translated_text=translated,
                label=label,
                score=round(score, 4),
            )
        )
    raw = score_sum / total if total > 0 else 0
    sentiment_score = round((raw + 1) / 2 * 100, 2)
    return SentimentResponse(
        il=il,
        results=results,
        sentiment_score=sentiment_score,
        haber_sayisi=total,
    )


@app.get("/health")
def health():
    return {"status": "ok", "model": "FinBERT"}


@app.post("/sentiment")
def analyze(request: SentimentRequest):
    return analyze_texts(request.il, request.texts)


@app.get("/sentiment/news/{il}")
def analyze_news(il: str, max_results: int = 10):
    """
    NewsAPI üzerinden ile özel haber çeker, FinBERT ile analiz eder.
    Sorgu: "<il> yatırım ekonomi"  →  Türkiye kaynaklı Türkçe haberler
    """
    try:
        # İl adı + ekonomi/yatırım anahtar kelimeleri ile Türkiye haberleri
        query = f'"{il}" AND (yatirim OR fabrika OR ihracat OR sanayi OR istihdam OR buyume OR GSYİH OR OSB)'
        response = newsapi.get_everything(
            q=query,
            sort_by="publishedAt",
            page_size=max_results,
            domains="dunya.com,bloomberght.com,hurriyet.com.tr,sabah.com.tr,milliyet.com.tr",
        )
        articles = response.get("articles", [])
    except Exception as e:
        # NewsAPI erişim sorunu varsa fallback olarak 50 döndür
        return {
            "il": il,
            "results": [],
            "sentiment_score": 50,
            "haber_sayisi": 0,
            "hata": str(e),
        }

    texts = []
    for article in articles:
        title = article.get("title") or ""
        description = article.get("description") or ""
        if not title:
            continue
        # Haber il ile ilgili mi? Başlık veya açıklamada il adı geçiyor mu kontrol et
        # NewsAPI zaten sorguyla filtreliyor; ek kontrol isteğe bağlı
        text = title + ". " + description[:300]
        texts.append(text)

    if not texts:
        return {
            "il": il,
            "results": [],
            "sentiment_score": 50,
            "haber_sayisi": 0,
        }

    return analyze_texts(il, texts[:max_results])


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8001)