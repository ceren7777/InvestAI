# -*- coding: utf-8 -*-

from fastapi import FastAPI
from pydantic import BaseModel
import httpx
import uvicorn

app = FastAPI()

class PromptRequest(BaseModel):
    prompt: str

@app.post("/ai/analyze")
async def analyze(req: PromptRequest):
    final_prompt = f"""
Do not think step by step.
Do not write reasoning.
Do not show internal thoughts.
Only write the final answer.
Always answer in Turkish.

{req.prompt}
"""

    async with httpx.AsyncClient(timeout=None) as client:
        response = await client.post(
            "http://localhost:11434/api/generate",
            json={
                "model": "qwen3:8b",
                "prompt": final_prompt,
                "stream": False,
                "think": False,
                "options": {
                    "temperature": 0.3,
                    "top_p": 0.9,
                    "num_predict": 900
                }
            }
        )

    data = response.json()
    result = data.get("response", "").strip()

    print("OLLAMA RESPONSE:")
    print(data)

    return {"result": result}

@app.get("/health")
def health():
    return {"status": "ok"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8002)