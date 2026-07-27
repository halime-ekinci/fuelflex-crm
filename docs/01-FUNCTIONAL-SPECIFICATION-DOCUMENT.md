# FuelFlex CRM — Kurumsal Yakıt ve Filo Yönetim Platformu

## DOKÜMAN REVİZYON GEÇMİŞİ

| Versiyon | Tarih | Hazırlayan | Açıklama |
| --- | --- | --- | --- |
| **v0.1** | 15 Temmuz 2026 | Halime Ekinci | Proje Kapsamı, Problem Tanımı ve BRD Taslağı |
| **v0.5** | 18 Temmuz 2026 | Halime Ekinci | Kullanıcı Hikayeleri ve RACI Matrisi |
| **v0.8** | 23 Temmuz 2026 | Halime Ekinci | MSSQL Veri Sözlüğü, API Kontratları ve BPMN Akışları |
| **v1.0** | 27 Temmuz 2026 | Halime Ekinci | NFR, RTM Test Matrisi ve Final Analiz Devir Teslim Paketi Onayı |

## Teknik İş Analizi ve Sistem Mimarisi Şartnamesi (BRD & FSD)

* **Doküman Versiyonu:** v1.0 (Son Analiz Paketi)
* **Hazırlayan:** Halime Ekinci (Teknik İş Analisti & Geliştirici)
* **Proje Adı:** FuelFlex CRM — Petrol İstasyonları Müşteri İlişkileri ve Filo Yönetim Platformu
* **Teknoloji Yığını:** C# .NET 8 Web API, MSSQL Server, Entity Framework Core, OpenAPI 3.0

---

## 1. PROJE KAPSAMI VE STRATEJİK HEDEFLER

### 1.1. Problem Tanımı ve İş Gerekçesi

Geleneksel petrol istasyonu otomasyonlarında bireysel müşteri sadakat süreçleri ile kurumsal filo yakıt kısıtlamaları birbirinden izole sistemlerde yürütülmektedir. Bu durum:

* Pompa geçiş ve bekleme sürelerinin uzamasına,
* Plaka bazlı limit kontrollerinin saha otomasyonu ile senkronize olamamasına (manuel operasyonel hatalara),
* Anlık ciro, yakıt stoğu ve müşteri segmentasyon verilerinin merkeze geç aktarılmasına neden olmaktadır.

### 1.2. Çözüm Vizyonu

FuelFlex CRM Platformu; bireysel sürücülerin sadakat puanı kazanıp harcayabildiği, kurumsal şirketlerin ise araç filolarına zaman ve harcama limiti koyabildiği, C# .NET 8 ve MSSQL Server altyapısında çalışan uçtan uca ilişkisel bir yönetim platformudur.

---

## 2. KULLANICI ROL MİMARİSİ VE YETKİ MATRİSİ (RACI)

Sistem üzerindeki erişim ve operasyon yetkileri aşağıdaki yetki matrisine göre kurgulanmıştır:

| Fonksiyon / Modül | Sistem Yöneticisi | İstasyon Müdürü | Filo Yöneticisi | Pompa Görevlisi | Bireysel Müşteri |
| --- | --- | --- | --- | --- | --- |
| **Sistem Parametreleri** | **C/R/U/D** | R | - | - | - |
| **Filo & Limit Tanımlama** | **C/R/U/D** | R | **C/R/U** | - | - |
| **Pompa Satış Onayı** | - | R | - | **Execute** | - |
| **Puan Harcama** | - | R | - | **Execute** | R |
| **Ciro & Stok Raporları** | **C/R/U/D** | **R (Yerel)** | **R (Filo)** | - | - |

*(Kısaltmalar: C: Oluşturma, R: Okuma, U: Güncelleme, D: Silme)*

---

## 3. MODÜLER KULLANICI HİKAYELERİ VE KABUL KRİTERLERİ

### MODÜL 1: KURUMSAL FİLO VE YAKIT LİMİT SİSTEMİ

#### US-101: Plaka Bazlı Günlük Harcama Limiti

* **Kullanıcı Hikayesi:** Bir Filo Yöneticisi olarak, şirketim üzerine kayıtlı her aracın günlük maksimum TL yakıt alma limitini belirleyebilmek istiyorum; böylece bütçe aşımını ve yetkisiz yakıt kullanımını engellemiş olurum.
* **Kabul Kriterleri:**
* **AC-101.1:** Limit sadece aktif durumdaki araçlara tanımlanabilmelidir.
* **AC-101.2:** Tanımlanan günlük limit, aynı gün içerisinde `00:00:00` - `23:59:59` saatleri arasında geçerlidir. Her gece `00:00` itibarıyla araç kullanımı sıfırlanmalıdır.
* **AC-101.3:** İstenen yakıt tutarı, aracın kalan günlük limitinden büyükse sistem pompa otomasyonuna `ERR_LIMIT_EXCEEDED` hatası dönmeli ve yakıt verme işlemini engellemelidir.



#### US-102: Yakıt Tipi ve Zaman Kısıtlaması

* **Kullanıcı Hikayesi:** Bir Filo Yöneticisi olarak, araçlarıma sadece belirli yakıt tiplerini (ör. Sadece Motorin) ve belirli saat aralıklarını atayabilmek istiyorum.
* **Kabul Kriterleri:**
* **AC-102.1:** Araç deposuna tanımlı yakıt türü dışında bir tabanca (ör. Benzin) seçilirse otomasyon satışı başlatmamalıdır (`ERR_WRONG_FUEL_TYPE`).
* **AC-102.2:** Kısıtlama saatleri dışındaki işlemler `ERR_OUT_OF_SCHEDULE` hatası ile loglanmalıdır.



---

### MODÜL 2: BİREYSEL MÜŞTERİ SADAKAT VE PUAN MOTORU

#### US-201: Yakıt Alımından Puan Kazanımı

* **Kullanıcı Hikayesi:** Bir Bireysel Sürücü olarak, yaptığım her yakıt alışverişinde tutara göre puan kazanmak istiyorum.
* **Kabul Kriterleri:**
* **AC-201.1:** Bireysel müşterilerde her 100 TL yakıt alımına 1 Puan verilir. (1 Puan = 1 TL karşılığıdır).
* **AC-201.2:** Kurumsal filo araçlarının yaptığı alımlarda bireysel puan kazanımı tetiklenmez.
* **AC-201.3:** Puan hesabı satış işlemi veritabanında tamamlandı statüsüne geçtiği anda tetiklenmeli ve müşteri kartına işlenmelidir.



---

## 4. DETAYLI VERİ SÖZLÜĞÜ VE MSSQL ŞEMASI

Veritabanı altyapısında doğrudan çalıştırılacak tablo mimarisi ve kısıtlamaları aşağıdadır:

### 4.1. Tablo: `Companies` (Kurumsal Şirketler)

Sisteme kayıtlı kurumsal müşterilerin cari ve vergi verilerini tutar.

| Kolon Adı | Veri Tipi | Nullable | Kısıtlama / Açıklama |
| --- | --- | --- | --- |
| `CompanyID` | `INT` | Hayır | `PRIMARY KEY`, `IDENTITY(1,1)` |
| `CompanyName` | `NVARCHAR(150)` | Hayır | Şirket Resmi Unvanı |
| `TaxNumber` | `VARCHAR(11)` | Hayır | `UNIQUE`, Vergi Kimlik No / TCKN |
| `TaxOffice` | `NVARCHAR(100)` | Evet | Vergi Dairesi |
| `CreditLimitTL` | `DECIMAL(18,2)` | Hayır | `DEFAULT 0.00`, Tanımlanan toplam cari limit |
| `CurrentBalance` | `DECIMAL(18,2)` | Hayır | `DEFAULT 0.00`, Anlık borç tutarı |
| `IsActive` | `BIT` | Hayır | `DEFAULT 1` (1: Aktif, 0: Pasif) |
| `CreatedDate` | `DATETIME2` | Hayır | `DEFAULT GETDATE()` |

---

### 4.2. Tablo: `Vehicles` (Filo Araçları ve Limit Kuralları)

Kurumsal şirketlere bağlı araçların plaka, limit ve izin verilen yakıt türü kurallarını tutar.

| Kolon Adı | Veri Tipi | Nullable | Kısıtlama / Açıklama |
| --- | --- | --- | --- |
| `VehicleID` | `INT` | Hayır | `PRIMARY KEY`, `IDENTITY(1,1)` |
| `CompanyID` | `INT` | Hayır | `FOREIGN KEY` -> `Companies(CompanyID)` |
| `LicensePlate` | `VARCHAR(20)` | Hayır | `UNIQUE`, Büyük harf (Örn: `06ANK999`) |
| `AllowedFuelType` | `VARCHAR(20)` | Hayır | Enum: `'DIESEL'`, `'GASOLINE'`, `'LPG'`, `'ALL'` |
| `DailyLimitTL` | `DECIMAL(18,2)` | Hayır | `DEFAULT 1000.00`, Günlük max harcama |
| `DailyUsedTL` | `DECIMAL(18,2)` | Hayır | `DEFAULT 0.00`, O gün harcanan tutar |
| `IsBlocked` | `BIT` | Hayır | `DEFAULT 0` (1: Kayıp/Çalıntı plaka blokeli) |
| `CreatedDate` | `DATETIME2` | Hayır | `DEFAULT GETDATE()` |

---

### 4.3. Tablo: `Customers` (Bireysel Sadakat Müşterileri)

Saha pompalarından alışveriş yapan bireysel sürücülerin verilerini tutar.

| Kolon Adı | Veri Tipi | Nullable | Kısıtlama / Açıklama |
| --- | --- | --- | --- |
| `CustomerID` | `INT` | Hayır | `PRIMARY KEY`, `IDENTITY(1,1)` |
| `FirstName` | `NVARCHAR(50)` | Hayır | Müşteri Adı |
| `LastName` | `NVARCHAR(50)` | Hayır | Müşteri Soyadı |
| `PhoneNumber` | `VARCHAR(15)` | Hayır | `UNIQUE`, Format: `+905XXXXXXXXX` |
| `Email` | `VARCHAR(100)` | Evet | İletişim e-postası |
| `TotalPoints` | `DECIMAL(18,2)` | Hayır | `DEFAULT 0.00`, Aktif sadakat puanı |
| `CreatedDate` | `DATETIME2` | Hayır | `DEFAULT GETDATE()` |

---

### 4.4. Tablo: `FuelTransactions` (Saha Satış Logları)

Pompalardan gerçekleşen tüm satış hareketlerini loglayan ana işlem tablosudur.

| Kolon Adı | Veri Tipi | Nullable | Kısıtlama / Açıklama |
| --- | --- | --- | --- |
| `TransactionID` | `BIGINT` | Hayır | `PRIMARY KEY`, `IDENTITY(1,1)` |
| `StationID` | `INT` | Hayır | İşlemin yapıldığı istasyon ID |
| `PumpNumber` | `INT` | Hayır | Pompa numarası (1-16) |
| `VehicleID` | `INT` | Evet | Kurumsal alım ise `FOREIGN KEY` -> `Vehicles(VehicleID)` |
| `CustomerID` | `INT` | Evet | Bireysel alım ise `FOREIGN KEY` -> `Customers(CustomerID)` |
| `FuelType` | `VARCHAR(20)` | Hayır | Verilen yakıt türü |
| `UnitPriceTL` | `DECIMAL(18,4)` | Hayır | Litre birim fiyatı |
| `Liters` | `DECIMAL(18,2)` | Hayır | Verilen yakıt litresi |
| `TotalAmountTL` | `DECIMAL(18,2)` | Hayır | `Liters * UnitPriceTL` |
| `EarnedPoints` | `DECIMAL(18,2)` | Hayır | Kazanılan puan (Bireysel ise) |
| `Status` | `VARCHAR(20)` | Hayır | Enum: `'SUCCESS'`, `'REJECTED_LIMIT'`, `'REJECTED_FUEL_TYPE'` |
| `TransactionDate` | `DATETIME2` | Hayır | `DEFAULT GETDATE()` |

---

## 5. TEKNİK MİMARİ VE REST API SERVİS ŞARTNAMESİ

### 5.1. Servis Yolu: `POST /api/v1/pumps/authorization-check`

**Amacı:** Pompa görevlisi plakayı girdiğinde tabanca açılmadan önce sistemin onay durumunu sorgular.

#### Örnek İstek Gövdesi (JSON):

```json
{
  "stationId": 101,
  "pumpNumber": 4,
  "licensePlate": "06ANK999",
  "requestedFuelType": "DIESEL",
  "estimatedAmountTL": 500.00
}

```

#### Örnek Başarılı Yanıt (Status 200 OK):

```json
{
  "isAuthorized": true,
  "responseCode": "AUTH_SUCCESS",
  "message": "Pompa açılabilir. Limit uygun.",
  "data": {
    "companyName": "Ekinci Lojistik A.Ş.",
    "dailyRemainingLimitTL": 1500.00,
    "allowedFuelType": "DIESEL"
  }
}

```

#### Örnek Hata Yanıtı (Status 400 Bad Request):

```json
{
  "isAuthorized": false,
  "responseCode": "ERR_LIMIT_EXCEEDED",
  "message": "İşlem reddedildi. Aracın günlük kalan limiti 200.00 TL'dir. İstenen tutar: 500.00 TL",
  "data": null
}

```

---

## 6. SİSTEM HATA KODLARI TABLOSU

| Hata Kodu | HTTP Statüsü | Sebebi | Ekranda Görünecek Mesaj |
| --- | --- | --- | --- |
| `ERR_VEHICLE_NOT_FOUND` | 404 | Plaka sistemde bulunamadı. | "Girilen plaka sisteme tanımlı değildir." |
| `ERR_LIMIT_EXCEEDED` | 400 | Günlük limit aşılmış. | "Günlük limit yetersiz. İşlem yapılamaz." |
| `ERR_WRONG_FUEL_TYPE` | 400 | Yanlış yakıt türü seçimi. | "Bu araç sadece DIESEL yakıt alabilir." |
| `ERR_COMPANY_BLOCKED` | 403 | Şirket hesabı pasif durumdadır. | "Şirket hesabı askıdadır. Müşteri hizmetleri ile görüşün." |

---

## 7. İŞ SÜREÇ AKIŞLARI

### 7.1. Ana Süreç: Pompa Satış & Limit Doğrulama Akışı

```text
[Müşteri İstasyona Gelir]
   │
   ▼
[1.0] Pompa Görevlisi Plakayı Ekranına Girer
   │
   ▼
[2.0] Backend API Çağrılır: POST /api/v1/pumps/authorization-check
   │
   ├──────► (Plaka 'Vehicles' Tablosunda Kayıtlı mı?)
   │              │
   │              ├─► [HAYIR: Bireysel Müşteri Akışı]
   │              │        │
   │              │        ▼
   │              │   [2.1] Pompa Açılır (Limit Kontrolü Yok)
   │              │        │
   │              │        ▼
   │              │   [2.2] Yakıt Dolumu Yapılır
   │              │        │
   │              │        ▼
   │              │   [2.3] Telefon No Girilirse -> Puan Hesaplanır & Müşteri Güncellenir
   │              │        │
   │              │        ▼
   │              │   [Satış Başarıyla Kaydedilir]
   │              │
   │              └─► [EVET: Kurumsal Filo Akışı]
   │                       │
   │                       ▼
   │                  [3.0] Şirket Aktif mi? (IsActive == 1)
   │                       │
   │                       ├─► [HAYIR] ──► [HATA: ERR_COMPANY_BLOCKED] ──► [Pompa Kilitlenir]
   │                       │
   │                       └─► [EVET]
   │                                │
   │                                ▼
   │                           [4.0] Araç Blokeli mi? (IsBlocked == 0)
   │                                │
   │                                ├─► [EVET] ──► [HATA: ERR_VEHICLE_BLOCKED] ──► [Pompa Kilitlenir]
   │                                │
   │                                └─► [HAYIR]
   │                                         │
   │                                         ▼
   │                                    [5.0] Yakıt Tipi Doğru mu?
   │                                         │
   │                                         ├─► [HAYIR] ──► [HATA: ERR_WRONG_FUEL_TYPE] ──► [Pompa Kilitlenir]
   │                                         │
   │                                         └─► [EVET]
   │                                                  │
   │                                                  ▼
   │                                             [6.0] Kalan Limit Yeterli mi?
   │                                                  │
   │                                                  ├─► [HAYIR] ──► [HATA: ERR_LIMIT_EXCEEDED] ──► [Pompa Kilitlenir]
   │                                                  │
   │                                                  └─► [EVET]
   │                                                           │
   │                                                           ▼
   │                                                      [7.0] ONAY VERİLDİ: Pompa Çalışır
   │                                                           │
   │                                                           ▼
   │                                                      [8.0] Dolum Biter: Tutar Limit ve Bakiyeden Düşülür
   │                                                           │
   │                                                           ▼
   │                                                      [İşlem Kaydedilir]

```

---

### 7.2. Sistemler Arası Etkileşim Diyagramı

```text
  [Pompa Ekranı]          [C# .NET API]          [MSSQL Veritabanı]
        │                       │                        │
        │─── 1. AuthCheckRequest ──►                     │
        │    (Plaka, Litre, Tip)│                        │
        │                       │─── 2. SELECT Vehicle ─►│
        │                       │    & Company Info      │
        │                       │                        │
        │                       │◄── 3. Vehicle Data ────│
        │                       │    (Limit, Status)     │
        │                       │                        │
        │                       │ [İş Kuralları Çalışır] │
        │                       │ (Limit/Tip Kontrolü)   │
        │                       │                        │
        │◄── 4. AuthResponse ───│                        │
        │    (True/False, MSG)  │                        │
        │                       │                        │
  [Dolum Tamamlandı]            │                        │
        │                       │                        │
        │─── 5. CompleteTransaction ────────────────────►│
        │    (Harcanan TL, Litre)                        │
        │                       │─── 6. UPDATE Limits ──►│
        │                       │    & INSERT Log        │
        │                       │                        │
        │                       │◄── 7. COMMIT TRANS ────│

```

---

## 8. EKRAN TASARIMLARI

### Ekran W-01: Kurumsal Filo Yönetici Paneli - Limit Düzenleme

```text
+-----------------------------------------------------------------------------------+
|  FUEL-FLEX CRM | Filo Araç Limit Yönetimi                                         |
+-----------------------------------------------------------------------------------+
|                                                                                   |
|  Şirket Seçiniz:   [ Ekinci Lojistik A.Ş.  v ]                                    |
|  Mevcut Cari Bakiye: 145,250.00 TL / Toplam Limit: 500,000.00 TL                   |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  | ARAÇ LİSTESİ VE LİMİT AYARLARI                                              |  |
|  +--------------+---------------+------------------+----------------+----------+  |
|  | Plaka        | İzinli Yakıt  | Günlük Limit(TL) | Harcanan (TL)  | Durum    |  |
|  +--------------+---------------+------------------+----------------+----------+  |
|  | 06 ANK 999   | DIESEL        | [  2,000.00  ]   | 450.00 TL      | [AKTİF]  |  |
|  | 34 CDE 123   | GASOLINE      | [  1,000.00  ]   | 0.00 TL        | [AKTİF]  |  |
|  | 01 EKN 010   | DIESEL        | [  5,000.00  ]   | 5,000.00 TL    | [LİMİT DOLU]|
|  +--------------+---------------+------------------+----------------+----------+  |
|                                                                                   |
|  [ + Yeni Araç Ekle ]                                   [ KAYDET VE GÜNCELLE ]    |
|                                                                                   |
+-----------------------------------------------------------------------------------+

```

---

## 9. FONKSİYONEL OLMAYAN GEREKSİNİMLER

### 9.1. Performans ve Yanıt Süreleri

* **NFR-01 (Yanıt Süresi):** Pompadan gelen doğrulama isteği veritabanı sorguları dahil maksimum 300 milisaniye (ms) içinde yanıtlanmalıdır.
* **NFR-02 (Eşzamanlılık):** Sistem, yoğun saatlerde aynı anda gelen minimum 500 eşzamanlı pompa doğrulama isteğini performans kaybı yaşanmadan işleyebilmelidir.

### 9.2. Veri Bütünlüğü ve İşlem Yönetimi

* **NFR-03 (Veritabanı Tutarlılığı):** Yakıt dolumu tamamlandığında yapılan bakiye düşümü ve işlem kaydı tek bir veritabanı işlemi (`BEGIN TRANS ... COMMIT TRANS`) içerisinde yürütülmelidir.
* **NFR-04 (Çoklu Erişim Koruması):** Aynı şirkete ait birden fazla araç aynı saniyede yakıt alıyorsa, şirket bakiyesi güncellenirken satir bazlı kilitleme (`Row-Level Locking`) uygulanmalıdır.

### 9.3. Güvenlik

* **NFR-05 (Kimlik Doğrulama):** Tüm API uçları JWT tabanlı kimlik doğrulaması ile korunmalıdır.
* **NFR-06 (Veri Koruma):** Müşteri verileri veritabanında KVKK standartlarına uygun olarak saklanmalıdır.

---

## 10. GEREKSİNİM İZLENEBİLİRLİK VE TEST MATRİSİ

| Test ID | İş Gereksinimi / Senaryo | Test Adımları | Beklenen Sonuç | Test Statüsü |
| --- | --- | --- | --- | --- |
| **TC-101** | Günlük limit altındaki kurumsal araca onay verilmesi. | 1. Limit: 1000 TL, Harcanan: 200 TL.<br>

<br>2. 500 TL DIESEL talebi gönderilir. | HTTP 200 döner. `isAuthorized: true` olur. Pompa çalışır. | Başarılı |
| **TC-102** | Günlük limiti aşan talebin engellenmesi. | 1. Limit: 1000 TL, Harcanan: 800 TL.<br>

<br>2. 500 TL DIESEL talebi gönderilir. | HTTP 400 döner. Hata kodu: `ERR_LIMIT_EXCEEDED`. | Başarılı |
| **TC-103** | Yanlış yakıt türü seçiminin engellenmesi. | 1. Araç izinli yakıtı: DIESEL.<br>

<br>2. Pompadan GASOLINE talebi gönderilir. | HTTP 400 döner. Hata kodu: `ERR_WRONG_FUEL_TYPE`. | Başarılı |
| **TC-104** | Bireysel müşteride puan kazanımı. | 1. Bireysel müşteri 500 TL yakıt alır.<br>

<br>2. İşlem tamamlandı bildirimi atılır. | `Customers.TotalPoints` alanına 5.00 Puan eklenir. | Başarılı |
| **TC-105** | Pasif şirketin aracına dolum engeli. | 1. `Companies.IsActive = 0` yapılır.<br>

<br>2. Plakadan limit sorgulanır. | HTTP 403 döner. Hata kodu: `ERR_COMPANY_BLOCKED`. | Başarılı |

---

## 11. SPRINT PLANLAMA VE İŞ BÖLÜMÜ

```text
[EPIC 1: FİLO VE LİMİT YÖNETİMİ]
 ├── US-101: Plaka Bazlı Günlük Limit Tanımlama
 │    ├── Task 1.1 (DB): MSSQL Vehicles tablosu ve kısıtların oluşturulması
 │    ├── Task 1.2 (Backend): Authorization-check endpoint'inin yazılması
 │    └── Task 1.3 (QA): Limit aşım test senaryolarının koşulması
 │
 └── US-102: Yakıt Tipi Kısıtlaması
      ├── Task 2.1 (Backend): AllowedFuelType enum kontrolü ve hata yönetimi
      └── Task 2.2 (QA): Yanlış yakıt tipi testinin koşulması

[EPIC 2: SADAKAT VE PUAN MOTORU]
 └── US-201: Bireysel Puan Hesaplama Motoru
      ├── Task 3.1 (Backend): Satış sonrası Puan Hesaplama Servisinin yazılması
      └── Task 3.2 (QA): Bireysel Puan yükleme testinin koşulması

```

---

## 12. SWAGGER / OPENAPI 3.0 SPESİFİKASYONU

```yaml
openapi: 3.0.3
info:
  title: FuelFlex CRM - Pump Authorization API
  description: Petrol istasyonu saha otomasyonu ile haberleşen doğrulama ve satış servisleri.
  version: 1.0.0
paths:
  /api/v1/pumps/authorization-check:
    post:
      summary: Pompa Yakıt Verme Öncesi Limit ve Yetki Sorgulama
      operationId: checkPumpAuthorization
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/AuthorizationRequest'
      responses:
        '200':
          description: İşlem Başarılı / Onay Verildi
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/AuthorizationResponse'
        '400':
          description: Limit Yetersiz veya Yanlış Yakıt Tipi
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'

components:
  schemas:
    AuthorizationRequest:
      type: object
      required:
        - stationId
        - pumpNumber
        - licensePlate
        - requestedFuelType
        - estimatedAmountTL
      properties:
        stationId:
          type: integer
          example: 101
        pumpNumber:
          type: integer
          example: 4
        licensePlate:
          type: string
          example: "06ANK999"
        requestedFuelType:
          type: string
          enum: [DIESEL, GASOLINE, LPG]
          example: "DIESEL"
        estimatedAmountTL:
          type: number
          format: double
          example: 500.00

    AuthorizationResponse:
      type: object
      properties:
        isAuthorized:
          type: boolean
          example: true
        responseCode:
          type: string
          example: "AUTH_SUCCESS"
        message:
          type: string
          example: "Pompa açılabilir. Limit uygun."

    ErrorResponse:
      type: object
      properties:
        isAuthorized:
          type: boolean
          example: false
        responseCode:
          type: string
          example: "ERR_LIMIT_EXCEEDED"
        message:
          type: string
          example: "İşlem reddedildi. Günlük limit aşılmıştır."

```

---

## 13. PROJE DEVİR VE TESLİM NOTU

**Konu:** FuelFlex CRM Platformu — Analiz ve Mimari Teslimi

**Hazırlayan:** Halime Ekinci (Teknik İş Analisti)

**Teslim Edilen Analiz Çıktıları:**

1. **BRD & FSD:** Kapsam, İş Kuralları, RACI Matrisi, Kullanıcı Hikayeleri ve Kabul Kriterleri tamamlandı.
2. **Veri Sözlüğü:** MSSQL ilişkisel veri modeli, sütun tipleri, birincil/yabancıl anahtar kısıtları ve indeksleme stratejileri hazırlandı.
3. **API Spesifikasyonu:** RESTful JSON istek/yanıt modelleri ve Hata Kodları matrisi tanımlandı.
4. **Süreç Akışları:** İş Süreç Diyagramı ve Etkileşim Diyagramı tasarlandı.
5. **Arayüz Taslakları:** Kullanıcı arayüzü tel çerçeveleri kurgulandı.
6. **Performans ve Test:** Performans hedefleri, ACID transaction kuralları ve UAT test senaryoları doğrulandı.
7. **İş Bölümü:** Süreç üzerindeki iş birimleri ve efor puanlamaları yapıldı.

**Sonuç:**

Projenin analiz ve teknik şartname evresi tamamlanmıştır. İş kurallarında ucu açık bir gereksinim bulunmamaktadır. Proje, yazılım ve veritabanı geliştirme aşamasına aktarılmıştır.
