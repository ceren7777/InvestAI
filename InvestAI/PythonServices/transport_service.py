# -*- coding: utf-8 -*-
import requests
import psycopg2
import time
import os
from dotenv import load_dotenv

load_dotenv(dotenv_path=os.path.join(os.path.dirname(__file__), '.env'))

DB_CONFIG = {
    "host": "localhost",
    "port": "5432",
    "dbname": "InvestAI",
    "user": "postgres",
    "password": "12345",
}

OVERPASS_URL = "https://overpass-api.de/api/interpreter"

ILLER = [
    ("Adana", 37.00, 35.32), ("Adiyaman", 37.76, 38.28),
    ("Afyonkarahisar", 38.75, 30.54), ("Agri", 39.72, 43.05),
    ("Aksaray", 38.37, 34.03), ("Amasya", 40.65, 35.83),
    ("Ankara", 39.92, 32.85), ("Antalya", 36.89, 30.71),
    ("Ardahan", 41.11, 42.70), ("Artvin", 41.18, 41.82),
    ("Aydin", 37.85, 27.84), ("Balikesir", 39.65, 27.88),
    ("Bartin", 41.64, 32.34), ("Batman", 37.88, 41.13),
    ("Bayburt", 40.26, 40.22), ("Bilecik", 40.14, 29.98),
    ("Bingol", 38.88, 40.50), ("Bitlis", 38.40, 42.11),
    ("Bolu", 40.74, 31.61), ("Burdur", 37.72, 30.29),
    ("Bursa", 40.19, 29.06), ("Canakkale", 40.15, 26.41),
    ("Cankiri", 40.60, 33.62), ("Corum", 40.55, 34.96),
    ("Denizli", 37.77, 29.09), ("Diyarbakir", 37.91, 40.23),
    ("Duzce", 40.84, 31.16), ("Edirne", 41.68, 26.56),
    ("Elazig", 38.67, 39.22), ("Erzincan", 39.75, 39.49),
    ("Erzurum", 39.91, 41.27), ("Eskisehir", 39.78, 30.52),
    ("Gaziantep", 37.06, 37.38), ("Giresun", 40.91, 38.39),
    ("Gumushane", 40.46, 39.48), ("Hakkari", 37.57, 43.74),
    ("Hatay", 36.20, 36.16), ("Igdir", 39.92, 44.04),
    ("Isparta", 37.76, 30.55), ("Istanbul", 41.01, 28.96),
    ("Izmir", 38.42, 27.14), ("Kahramanmaras", 37.59, 36.94),
    ("Karabuk", 41.20, 32.63), ("Karaman", 37.18, 33.22),
    ("Kars", 40.61, 43.10), ("Kastamonu", 41.38, 33.78),
    ("Kayseri", 38.73, 35.49), ("Kilis", 36.72, 37.12),
    ("Kirikkale", 39.85, 33.51), ("Kirklareli", 41.74, 27.22),
    ("Kirsehir", 39.15, 34.17), ("Kocaeli", 40.85, 29.88),
    ("Konya", 37.87, 32.49), ("Kutahya", 39.42, 29.98),
    ("Malatya", 38.35, 38.31), ("Manisa", 38.62, 27.43),
    ("Mardin", 37.31, 40.74), ("Mersin", 36.80, 34.63),
    ("Mugla", 37.21, 28.36), ("Mus", 38.73, 41.49),
    ("Nevsehir", 38.62, 34.72), ("Nigde", 37.97, 34.68),
    ("Ordu", 40.98, 37.88), ("Osmaniye", 37.07, 36.25),
    ("Rize", 41.02, 40.52), ("Sakarya", 40.69, 30.43),
    ("Samsun", 41.29, 36.33), ("Siirt", 37.93, 41.95),
    ("Sinop", 42.02, 35.15), ("Sivas", 39.75, 37.02),
    ("Sanliurfa", 37.16, 38.80), ("Sirnak", 37.52, 42.46),
    ("Tekirdag", 40.98, 27.51), ("Tokat", 40.31, 36.55),
    ("Trabzon", 41.00, 39.72), ("Tunceli", 39.11, 39.55),
    ("Usak", 38.68, 29.41), ("Van", 38.49, 43.38),
    ("Yalova", 40.65, 29.27), ("Yozgat", 39.82, 34.81),
    ("Zonguldak", 41.45, 31.80),
]

def overpass_query(query):
    try:
        resp = requests.post(OVERPASS_URL, data={"data": query}, timeout=30)
        resp.raise_for_status()
        return resp.json()
    except Exception as e:
        print(f"  OSM hata: {e}")
        return {"elements": []}

def check_airport(lat, lon, radius=80000):
    q = f"[out:json][timeout:25];(node[\"aeroway\"=\"aerodrome\"](around:{radius},{lat},{lon});way[\"aeroway\"=\"aerodrome\"](around:{radius},{lat},{lon}););out count;"
    data = overpass_query(q)
    elems = [e for e in data.get("elements", []) if e.get("type") in ("node", "way")]
    return len(elems) > 0

def check_motorway(lat, lon, radius=60000):
    q = f"[out:json][timeout:25];way[\"highway\"=\"motorway\"](around:{radius},{lat},{lon});out count;"
    data = overpass_query(q)
    return len(data.get("elements", [])) > 0

def check_railway(lat, lon, radius=60000):
    q = f"[out:json][timeout:25];(way[\"railway\"=\"rail\"](around:{radius},{lat},{lon});way[\"railway\"=\"light_rail\"](around:{radius},{lat},{lon}););out count;"
    data = overpass_query(q)
    return len(data.get("elements", [])) > 0

def check_port(lat, lon, radius=80000):
    q = f"[out:json][timeout:25];(node[\"harbour\"=\"yes\"](around:{radius},{lat},{lon});node[\"seamark:type\"=\"harbour\"](around:{radius},{lat},{lon});way[\"waterway\"=\"dock\"](around:{radius},{lat},{lon}););out count;"
    data = overpass_query(q)
    return len(data.get("elements", [])) > 0

def hesapla_ulasim_skoru(havaalani, otoyol, demiryolu, liman):
    skor = 0
    if havaalani: skor += 35
    if otoyol: skor += 30
    if demiryolu: skor += 25
    if liman: skor += 10
    return skor

def create_table(cur):
    cur.execute("""
        CREATE TABLE IF NOT EXISTS ulasim_altyapi (
            id SERIAL PRIMARY KEY,
            il VARCHAR(100) NOT NULL UNIQUE,
            havaalani BOOLEAN DEFAULT FALSE,
            otoyol BOOLEAN DEFAULT FALSE,
            demiryolu BOOLEAN DEFAULT FALSE,
            liman BOOLEAN DEFAULT FALSE,
            ulasim_skoru INTEGER DEFAULT 0,
            kaynak VARCHAR(50) DEFAULT 'OSM Overpass API',
            guncelleme_tarihi TIMESTAMP DEFAULT NOW()
        );
    """)

def main():
    print("=== OSM Ulasim Verisi Cekici ===")
    print(f"Toplam {len(ILLER)} il islenecek\n")

    conn = psycopg2.connect(**DB_CONFIG)
    cur = conn.cursor()
    create_table(cur)
    conn.commit()

    for il, lat, lon in ILLER:
        print(f"[{il}] isleniyor...")
        cur.execute("SELECT id FROM ulasim_altyapi WHERE il = %s", (il,))
        if cur.fetchone():
            print(f"  -> Zaten var, atlaniyor.")
            continue

        havaalani = check_airport(lat, lon)
        time.sleep(1)
        otoyol = check_motorway(lat, lon)
        time.sleep(1)
        demiryolu = check_railway(lat, lon)
        time.sleep(1)
        liman = check_port(lat, lon)
        time.sleep(1)

        skor = hesapla_ulasim_skoru(havaalani, otoyol, demiryolu, liman)

        cur.execute("""
            INSERT INTO ulasim_altyapi (il, havaalani, otoyol, demiryolu, liman, ulasim_skoru)
            VALUES (%s, %s, %s, %s, %s, %s)
            ON CONFLICT (il) DO UPDATE SET
                havaalani = EXCLUDED.havaalani,
                otoyol = EXCLUDED.otoyol,
                demiryolu = EXCLUDED.demiryolu,
                liman = EXCLUDED.liman,
                ulasim_skoru = EXCLUDED.ulasim_skoru,
                guncelleme_tarihi = NOW()
        """, (il, havaalani, otoyol, demiryolu, liman, skor))
        conn.commit()

        print(f"  Havaalani: {havaalani} | Otoyol: {otoyol} | Demiryolu: {demiryolu} | Liman: {liman} | Skor: {skor}")

    cur.close()
    conn.close()
    print("\nTamamlandi!")

if __name__ == "__main__":
    main()