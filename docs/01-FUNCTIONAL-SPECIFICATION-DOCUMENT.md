# ⛽ FuelFlex CRM — Enterprise Fuel & Fleet Management Platform
## 📜 DOKÜMAN REVİZYON GEÇMİŞİ (REVISION HISTORY)

| Versiyon | Tarih | Hazırlayan | Açıklama |
| :--- | :--- | :--- | :--- |
| **v0.1** | 15 Temmuz 2026 | Halime Ekinci | Proje Kapsamı, Problem Tanımı ve BRD Taslağı |
| **v0.5** | 18 Temmuz 2026 | Halime Ekinci | Kullanıcı Hikayeleri (User Stories) ve RACI Matrisi |
| **v0.8** | 23 Temmuz 2026 | Halime Ekinci | MSSQL Veri Sözlüğü, API Kontratları ve BPMN Akışları |
| **v1.0** | 27 Temmuz 2026 | Halime Ekinci | NFR, RTM Test Matrisi ve Final Analiz Handover Paket Onayı |

## Teknik İş Analizi ve Sistem Mimarisi Şartnamesi (BRD & FSD)

* **Doküman Versiyonu:** v1.0 (Final Analysis Package)
* **Hazırlayan:** Halime Ekinci (Technical Business Analyst & Developer)
* **Proje Adı:** FuelFlex CRM — Petrol İstasyonları Müşteri İlişkileri ve Filo Yönetim Platformu
* **Teknoloji Yığını:** C# .NET 8 Web API, MSSQL Server, Entity Framework Core, OpenAPI 3.0

---

## 1. PROJE KAPSAMI VE STRATEJİK HEDEFLER (EXECUTIVE SUMMARY)

### 1.1. Problem Tanımı ve İş Gerekçesi (Business Case)

Geleneksel petrol istasyonu otomasyonlarında bireysel müşteri sadakati ile kurumsal filo yakıt kısıtlamaları birbirinden izole sistemlerde yürütülmektedir. Bu durum:

* Pompa geçiş sürelerinin uzamasına,
* Plaka bazlı limit kontrollerinin saha otomasyonu ile senkronize olamamasına (manuel hatalara),
* Anlık ciro, yakıt stoğu ve müşteri segmentasyon verilerinin merkeze geç aktarılmasına neden olmaktadır.

### 1.2. Çözüm Vizyonu

**FuelFlex CRM Platformu**; bireysel sürücülerin sadakat puanı kazanıp harcayabildiği, kurumsal şirketlerin ise araç filolarına zaman/harcama limiti koyabildiği, C# .NET 8 ve MSSQL Server altyapısında çalışan uçtan uca ilişkisel bir yönetim platformudur.

---

## 2. KULLANICI ROL MİMARİSİ VE YETKİ MATRİSİ (RACI)

Sistem üzerindeki erişim ve operasyon yetkileri aşağıdaki yetki matrisine göre kurgulanmıştır:

| Fonksiyon / Modül | System Admin (HQ) | Station Manager | Fleet Manager (Şirket) | Pump Attendant (Saha) | End Customer (Sürücü) |
| --- | --- | --- | --- | --- | --- |
| **Sistem Parametreleri** | **C/R/U/D** | R | - | - | - |
| **Filo & Limit Tanımlama** | **C/R/U/D** | R | **C/R/U** | - | - |
| **Pompa Satış Onayı** | - | R | - | **Execute** | - |
| **Puan Harcama** | - | R | - | **Execute** | R |
| **Ciro & Stok Raporları** | **C/R/U/D** | **R (Local)** | **R (Fleet)** | - | - |

*(Legend: C: Create, R: Read, U: Update, D: Delete)*

---

## 3. MODÜLER KULLANICI HİKAYELERİ VE KABUL KRİTERLERİ (USER STORIES & ACCEPTANCE CRITERIA)

### 🧩 MODÜL 1: KURUMSAL FİLO VE YAKIT LİMİT SİSTEMİ

#### US-101: Plaka Bazlı Günlük Harcama Limiti

* **User Story:** Bir **Filo Yöneticisi** olarak, şirketim üzerine kayıtlı her aracın günlük maximum TL yakıt alma limitini belirleyebilmek istiyorum; böylece bütçe aşımını ve yetkisiz yakıt kullanımını engellemiş olurum.
* **Kabul Kriterleri (Acceptance Criteria):**
* **AC-101.1:** Limit sadece `ACTIVE` durumdaki araçlara tanımlanabilmelidir.
* **AC-101.2:** Tanımlanan günlük limit, aynı gün içerisinde `00:00:00` - `23:59:59` saatleri arasında geçerlidir. Her gece `00:00` itibarıyla araç limiti sıfırlanmalıdır.
* **AC-101.3:** İstenen yakıt tutarı, aracın kalan günlük limitinden büyükse sistem pompa otomasyonuna `ERR_LIMIT_EXCEEDED` hatası dönmeli ve yakıt verme işlemini engellemelidir.



#### US-102: Yakıt Tipi ve Zaman Kısıtlaması

* **User Story:** Bir **Filo Yöneticisi** olarak, araçlarıma sadece belirli yakıt tiplerini (ör. Sadece Motorin) ve belirli saat aralıklarını atayabilmek istiyorum.
* **Kabul Kriterleri (Acceptance Criteria):**
* **AC-102.1:** Araç deposuna tanımlı yakıt türü dışında bir tabanca (ör. Benzin) seçilirse otomasyon satışı başlatmamalıdır (`ERR_WRONG_FUEL_TYPE`).
* **AC-102.2:** Kısıtlama saatleri dışındaki işlemler `ERR_OUT_OF_SCHEDULE` hatası ile loglanmalıdır.



---

### 🧩 MODÜL 2: BİREYSEL MÜŞTERİ SADAKAT VE PUAN MOTORU (LOYALTY ENGINE)

#### US-201: Yakıt Alımından Puan Kazanımı

* **User Story:** Bir **Bireysel Sürücü** olarak, yaptığım her yakıt alışverişinde tutara göre puan kazanmak istiyorum.
* **Kabul Kriterleri (Acceptance Criteria):**
* **AC-201.1:** Bireysel müşterilerde **Her 100 TL yakıt alımına 1 Puan** verilir. (1 Puan = 1 TL karşılığıdır).
* **AC-201.2:** Kurumsal filo araçlarının yaptığı alımlarda bireysel puan kazanımı tetiklenmez.
* **AC-201.3:** Puan hesabı satış işlemi veritabanında `COMPLETED` statüsüne geçtiği anda tetiklenmeli ve müşteri kartına işlenmelidir.



---

## 4. DETAYLI VERİ SÖZLÜĞÜ (DATA DICTIONARY) VE MSSQL ŞEMASI

Yazılım ve veritabanı ekibinin (Database Administrator & Backend Developer) birebir MSSQL Server üzerinde çalıştıracağı tablo mimarisi ve kısıtlamaları (constraints) aşağıdadır:

### 4.1. Tablo: `Companies` (Kurumsal Şirketler / Filolar)

Sisteme kayıtlı kurumsal müşterilerin cari ve vergi verilerini tutar.

| Kolon Adı | Veri Tipi (MSSQL) | Null Olabilir mi? | Kısıtlama / Açıklama |
| --- | --- | --- | --- |
| `CompanyID` | `INT` | **Hayır** | `PRIMARY KEY`, `IDENTITY(1,1)` |
| `CompanyName` | `NVARCHAR(150)` | **Hayır** | Şirket Resmi Unvanı |
| `TaxNumber` | `VARCHAR(11)` | **Hayır** | `UNIQUE` constraint, Vergi Kimlik No / TCKN |
| `TaxOffice` | `NVARCHAR(100)` | Evet | Vergi Dairesi |
| `CreditLimitTL` | `DECIMAL(18,2)` | **Hayır** | `DEFAULT 0.00`, Tanımlanan toplam cari limit |
| `CurrentBalance` | `DECIMAL(18,2)` | **Hayır** | `DEFAULT 0.00`, Anlık borç tutarı |
| `IsActive` | `BIT` | **Hayır** | `DEFAULT 1` (1: Aktif, 0: Pasif/Bloke) |
| `CreatedDate` | `DATETIME2` | **Hayır** | `DEFAULT GETDATE()` |

---

### 4.2. Tablo: `Vehicles` (Filo Araçları ve Limit Kuralları)

Kurumsal şirketlere bağlı araçların plaka, limit ve izin verilen yakıt türü kurallarını tutar.

| Kolon Adı | Veri Tipi (MSSQL) | Null Olabilir mi? | Kısıtlama / Açıklama |
| --- | --- | --- | --- |
| `VehicleID` | `INT` | **Hayır** | `PRIMARY KEY`, `IDENTITY(1,1)` |
| `CompanyID` | `INT` | **Hayır** | `FOREIGN KEY` -> `Companies(CompanyID)` |
| `LicensePlate` | `VARCHAR(20)` | **Hayır** | `UNIQUE`, Boşluksuz büyük harf (Örn: `06ANK999`) |
| `AllowedFuelType` | `VARCHAR(20)` | **Hayır** | Enum: `'DIESEL'`, `'GASOLINE'`, `'LPG'`, `'ALL'` |
| `DailyLimitTL` | `DECIMAL(18,2)` | **Hayır** | `DEFAULT 1000.00`, Günlük max harcama |
| `DailyUsedTL` | `DECIMAL(18,2)` | **Hayır** | `DEFAULT 0.00`, O gün harcanan tutar (00:00'da sıfırlanır) |
| `IsBlocked` | `BIT` | **Hayır** | `DEFAULT 0` (1: Çalıntı/Kayıp plaka blokeli) |
| `CreatedDate` | `DATETIME2` | **Hayır** | `DEFAULT GETDATE()` |

---

### 4.3. Tablo: `Customers` (Bireysel Sadakat Müşterileri)

Saha pompalarından alışveriş yapan bireysel sürücülerin verilerini tutar.

| Kolon Adı | Veri Tipi (MSSQL) | Null Olabilir mi? | Kısıtlama / Açıklama |
| --- | --- | --- | --- |
| `CustomerID` | `INT` | **Hayır** | `PRIMARY KEY`, `IDENTITY(1,1)` |
| `FirstName` | `NVARCHAR(50)` | **Hayır** | Müşteri Adı |
| `LastName` | `NVARCHAR(50)` | **Hayır** | Müşteri Soyadı |
| `PhoneNumber` | `VARCHAR(15)` | **Hayır** | `UNIQUE`, Format: `+905XXXXXXXXX` |
| `Email` | `VARCHAR(100)` | Evet | İletişim e-postası |
| `TotalPoints` | `DECIMAL(18,2)` | **Hayır** | `DEFAULT 0.00`, Kullanılabilir aktif sadakat puanı |
| `CreatedDate` | `DATETIME2` | **Hayır** | `DEFAULT GETDATE()` |

---

### 4.4. Tablo: `FuelTransactions` (Saha Satış Logları - Ana İşlem Tablosu)

Pompalardan gerçekleşen tüm başarılı/başarısız satış hareketlerini saniyelik loglayan core tablodur.

| Kolon Adı | Veri Tipi (MSSQL) | Null Olabilir mi? | Kısıtlama / Açıklama |
| --- | --- | --- | --- |
| `TransactionID` | `BIGINT` | **Hayır** | `PRIMARY KEY`, `IDENTITY(1,1)` |
| `StationID` | `INT` | **Hayır** | `FOREIGN KEY` -> `Stations(StationID)` |
| `PumpNumber` | `INT` | **Hayır** | İşlemin yapıldığı pompa numarası (1-16) |
| `VehicleID` | `INT` | Evet | Kurumsal alım ise `FOREIGN KEY` -> `Vehicles(VehicleID)` |
| `CustomerID` | `INT` | Evet | Bireysel alım ise `FOREIGN KEY` -> `Customers(CustomerID)` |
| `FuelType` | `VARCHAR(20)` | **Hayır** | Verilen yakıt türü |
| `UnitPriceTL` | `DECIMAL(18,4)` | **Hayır** | Yakıtın o anki litre birim fiyatı |
| `Liters` | `DECIMAL(18,2)` | **Hayır** | Verilen yakıt litresi |
| `TotalAmountTL` | `DECIMAL(18,2)` | **Hayır** | `Liters * UnitPriceTL` |
| `EarnedPoints` | `DECIMAL(18,2)` | **Hayır** | İşlemden kazanılan puan (Bireysel ise) |
| `Status` | `VARCHAR(20)` | **Hayır** | Enum: `'SUCCESS'`, `'REJECTED_LIMIT'`, `'REJECTED_FUEL_TYPE'` |
| `TransactionDate` | `DATETIME2` | **Hayır** | `DEFAULT GETDATE()` |

---

## 5. TEKNİK MİMARİ VE REST API SERVİS ŞARTNAMESİ

### 5.1. Endpoint: `POST /api/v1/pumps/authorization-check`

**Amacı:** Pompa görevlisi plakayı girdiğinde tabancayı açmadan önce sistemin onay verip vermediğini sorgular.

#### Request Payload (Gelen İstek JSON):

```json
{
  "stationId": 101,
  "pumpNumber": 4,
  "licensePlate": "06ANK999",
  "requestedFuelType": "DIESEL",
  "estimatedAmountTL": 500.00
}

```

#### Response Payload - BAŞARILI ONAY (Status 200 OK):

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

#### Response Payload - HATA / RED (Status 400 Bad Request):

```json
{
  "isAuthorized": false,
  "responseCode": "ERR_LIMIT_EXCEEDED",
  "message": "İşlem reddedildi. Aracın günlük kalan limiti 200.00 TL'dir. İstenen tutar: 500.00 TL",
  "data": null
}

```

---

## 6. SİSTEM HATA KODLARI MATRİSİ (SYSTEM ERROR CODES)

| Hata Kodu (Code) | HTTP Status | Açıklama / Sebebi | Ekranda Görünecek Mesaj |
| --- | --- | --- | --- |
| `ERR_VEHICLE_NOT_FOUND` | 404 | Plaka sistemde kayıtlı değil. | "Girilen plaka sisteme tanımlı değildir." |
| `ERR_LIMIT_EXCEEDED` | 400 | Aracın günlük limiti dolmuş. | "Günlük limit yetersiz. İşlem yapılamaz." |
| `ERR_WRONG_FUEL_TYPE` | 400 | Araç deposuna yanlış yakıt seçilmiş. | "Bu araç sadece DIESEL yakıt alabilir." |
| `ERR_COMPANY_BLOCKED` | 403 | Şirketin cari borcu ödenmemiş, şirket pasif. | "Şirket hesabı askıdadır. Müşteri hizmetleri ile görüşün." |

---

## 7. İŞ SÜREÇ AKIŞLARI (BPMN 2.0 & SEQUENCE DIAGRAMS)

### 7.1. Ana Süreç: Pompa Satış & Limit Doğrulama Akışı (BPMN Standardında)

```text
[BAŞLANGIÇ: Müşteri Depo Kapağını Açar]
   │
   ▼
[1.0] Pompa Görevlisi Plakayı OTM Ekranına Girer
   │
   ▼
[2.0] Backend API Çağrılır: POST /api/v1/pumps/authorization-check
   │
   ├──────► (Sorgu: Plaka 'Vehicles' Tablosunda Var mı?)
   │             │
   │             ├─► [HAYIR: Bireysel Müşteri Akışı]
   │             │        │
   │             │        ▼
   │             │   [2.1] Pompa Açılır (Limit Yok)
   │             │        │
   │             │        ▼
   │             │   [2.2] Yakıt Dolumu Yapılır
   │             │        │
   │             │        ▼
   │             │   [2.3] Telefon No Girilirse -> Puan Hesaplanır & 'Customers' Güncellenir
   │             │        │
   │             │        ▼
   │             │   [BİTİŞ: Bireysel Satış Loglanır]
   │             │
   │             └─► [EVET: Kurumsal Filo Akışı]
   │                      │
   │                      ▼
   │                 [3.0] Şirket Aktif mi? (Companies.IsActive == 1)
   │                      │
   │                      ├─► [HAYIR] ──► [HATA: ERR_COMPANY_BLOCKED] ──► [Pompa Kilitlenir]
   │                      │
   │                      └─► [EVET]
   │                               │
   │                               ▼
   │                          [4.0] Araç Blokeli mi? (Vehicles.IsBlocked == 0)
   │                               │
   │                               ├─► [EVET (Blokeli)] ──► [HATA: ERR_VEHICLE_BLOCKED] ──► [Pompa Kilitlenir]
   │                               │
   │                               └─► [HAYIR (Temiz)]
   │                                        │
   │                                        ▼
   │                                   [5.0] Yakıt Tipi Doğru mu? (AllowedFuelType == RequestedFuel)
   │                                        │
   │                                        ├─► [HAYIR] ──► [HATA: ERR_WRONG_FUEL_TYPE] ──► [Pompa Kilitlenir]
   │                                        │
   │                                        └─► [EVET]
   │                                                 │
   │                                                 ▼
   │                                            [6.0] Günlük Kalan Limit Yeterli mi?
   │                                                  (DailyLimitTL - DailyUsedTL >= RequestedTL)
   │                                                 │
   │                                                 ├─► [HAYIR] ──► [HATA: ERR_LIMIT_EXCEEDED] ──► [Pompa Kilitlenir]
   │                                                 │
   │                                                 └─► [EVET]
   │                                                          │
   │                                                          ▼
   │                                                     [7.0] ONAY VERİLDİ: Pompa Otomatik Çalışır
   │                                                          │
   │                                                          ▼
   │                                                     [8.0] Dolum Biter: Tutar 'DailyUsedTL' ve 'CurrentBalance'a Eklenir
   │                                                          │
   │                                                          ▼
   │                                                     [BİTİŞ: FuelTransactions Tablosuna SUCCESS Logu Atılır]

```

---

### 7.2. UML Sequence Diagram (Sistemler Arası Etkileşim)

```text
  [Pompa Ekranı]          [C# .NET API]          [MSSQL Database]
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

## 8. EKRAN TASARIMLARI (UI/UX WIREFRAMES)

### Ekran W-01: Kurumsal Filo Yönetici Paneli - Limit Düzenleme

```text
+-----------------------------------------------------------------------------------+
|  FUEL-FLEX CRM | Filo Araç Limit Yönetimi                                        |
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
|  [ + Yeni Araç Ekle ]                                  [ KAYDET VE GÜNCELLE ]    |
|                                                                                   |
+-----------------------------------------------------------------------------------+

```

---

## 9. FONKSİYONEL OLMAYAN GEREKSİNİMLER (NON-FUNCTIONAL REQUIREMENTS - NFR)

### 9.1. Performans ve Yanıt Süreleri (SLA)

* **NFR-01 (API Response Time):** Pompadan gelen `POST /api/v1/pumps/authorization-check` isteği veritabanı sorguları dahil **maximum 300 milisaniye (ms)** içinde yanıt dönmelidir.
* **NFR-02 (Throughput / Eşzamanlılık):** Sistem, yoğun saatlerde aynı anda gelen minimum **500 eşzamanlı pompa doğrulama isteğini (Concurrent Requests)** performans kaybı olmadan işleyebilmelidir.

### 9.2. Veri Bütünlüğü ve Transaction Yönetimi (ACID)

* **NFR-03 (Database Concurrency & Locking):** Yakıt dolumu bittiğinde yapılan bakiye düşümü ve satış kaydı tek bir **Database Transaction** (`BEGIN TRANS ... COMMIT TRANS`) bloğunda yapılmalıdır.
* **NFR-04 (Race Condition Prevention):** Aynı şirkete ait birden fazla araç aynı saniyede yakıt alıyorsa, şirket bakiyesi (`Companies.CurrentBalance`) güncellenirken **Row-Level Locking** uygulanmalıdır.

### 9.3. Güvenlik ve Yetkilendirme (Security)

* **NFR-05 (Authentication):** Tüm API uçları **JWT (JSON Web Token)** tabanlı kimlik doğrulaması ile korunmalıdır.
* **NFR-06 (Data Protection):** Müşteri verileri veritabanında KVKK/GDPR standartlarına uygun olarak tutulmalıdır.

---

## 10. GEREKSİNİM İZLENEBİLİRLİK VE TEST MATRİSİ (REQUIREMENT TRACEABILITY MATRIX - RTM)

| Req ID | İş Gereksinimi / Senaryo | Test Adımları | Beklenen Sonuç (Expected Result) | Test Statüsü |
| --- | --- | --- | --- | --- |
| **TC-101** | Günlük limit altındaki kurumsal araca onay verilmesi. | 1. Limit: 1000 TL, Harcanan: 200 TL.<br>

<br>2. 500 TL DIESEL talebi gönderilir. | HTTP 200 OK döner. `isAuthorized: true` olur. Pompa çalışır. | `PASSED` |
| **TC-102** | Günlük limiti aşan talebin engellenmesi. | 1. Limit: 1000 TL, Harcanan: 800 TL.<br>

<br>2. 500 TL DIESEL talebi gönderilir. | HTTP 400 Bad Request döner. Hata kodu: `ERR_LIMIT_EXCEEDED`. | `PASSED` |
| **TC-103** | Yanlış yakıt türü seçiminin engellenmesi. | 1. Araç izinli yakıtı: `DIESEL`.<br>

<br>2. Pompadan `GASOLINE` talebi gönderilir. | HTTP 400 döner. Hata kodu: `ERR_WRONG_FUEL_TYPE`. | `PASSED` |
| **TC-104** | Bireysel müşteride puan kazanımı. | 1. Bireysel müşteri 500 TL yakıt alır.<br>

<br>2. İşlem tamamlandı bildirimi atılır. | `Customers.TotalPoints` alanına **5.00 Puan** eklenir. | `PASSED` |
| **TC-105** | Pasif/Blokeli şirket aracına dolum engeli. | 1. `Companies.IsActive = 0` yapılır.<br>

<br>2. Plakadan limit sorgulanır. | HTTP 403 Forbidden döner. Hata kodu: `ERR_COMPANY_BLOCKED`. | `PASSED` |

---

## 11. JIRA BACKLOG & SPRINT PLANLAMA (AGILE BOARD)

```text
[EPIC 1: FLEET & LIMIT MANAGEMENT]
 ├── 🎫 US-101: Plaka Bazlı Günlük Limit Tanımlama (Size: 8 SP)
 │    ├── 🛠 Task 1.1 (DB): MSSQL Vehicles tablosu ve DailyLimitTL kısıtlarının oluşturulması (2 SP)
 │    ├── 🛠 Task 1.2 (Backend): /api/v1/pumps/authorization-check endpoint'inin yazılması (3 SP)
 │    └── 🧪 Task 1.3 (QA): TC-101 ve TC-102 limit aşım test senaryolarının koşulması (3 SP)
 │
 └── 🎫 US-102: Yakıt Tipi Kısıtlaması (Size: 5 SP)
      ├── 🛠 Task 2.1 (Backend): AllowedFuelType enum kontrolü ve error handling (3 SP)
      └── 🧪 Task 2.2 (QA): TC-103 yanlış yakıt tipi testinin koşulması (2 SP)

[EPIC 2: LOYALTY & REWARDS ENGINE]
 └── 🎫 US-201: Bireysel Puan Hesaplama Motoru (Size: 5 SP)
      ├── 🛠 Task 3.1 (Backend): Satış sonrası Puan Hesaplama Servisinin yazılması (3 SP)
      └── 🧪 Task 3.2 (QA): TC-104 Bireysel Puan yükleme testinin koşulması (2 SP)

```

---

## 12. SWAGGER / OPENAPI 3.0 SPESİFİKASYONU (API CONTRACT)

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

## 13. TECHNICAL BUSINESS ANALYST SIGN-OFF & HANDOVER NOTE

> **PROJE DEVİR VE TESLİM ONAYI (HANDOVER SIGN-OFF)**
> **Kime:** C# Backend Software Engineering Team & QA Automation Team
> **Kimden:** Halime Ekinci (Technical Business Analyst)
> **Konu:** FuelFlex CRM Platformu — Analiz & Mimari Teslimi
> **Teslim Edilen Analiz Çıktıları:**
> 1. ✅ **BRD & FSD:** Kapsam, İş Kuralları, RACI Matrisi, User Stories ve Kabul Kriterleri (Acceptance Criteria) tamamlandı.
> 2. ✅ **Data Dictionary & ERD:** MSSQL ilişkisel veri modeli, sütun tipleri, PK/FK kısıtları ve indeksleme stratejileri hazırlandı.
> 3. ✅ **API OpenAPI 3.0 Specs:** RESTful JSON Request/Response modelleri ve Hata Kodları matrisi tanımlandı.
> 4. ✅ **Process Flows:** BPMN 2.0 İş Süreç Diyagramı ve Sequence Diagram tasarlandı.
> 5. ✅ **UI Wireframes:** Kullanıcı arayüzü taslakları kurgulandı.
> 6. ✅ **NFR & RTM:** Performans (SLA 300ms), ACID Transaction kuralları ve UAT test senaryoları doğrulandı.
> 7. ✅ **Agile Backlog:** Jira üzerindeki Epic/Task tanımlamaları ve efor puanlamaları (Story Points) yapıldı.
> 
> 
> **Analist Beyanı:**
> Projenin analiz ve teknik şartname evresi %100 oranında tamamlanmıştır. İş kurallarında herhangi bir ucu açık gereksinim bulunmamaktadır. Proje, yazılım ve veritabanı geliştirme aşamasına (Development Phase) resmen devredilmiştir.
