# ⛽ FuelFlex CRM — Akıllı Pompa Limit, Yetkilendirme ve Sadakat Puan Engine (v1.0)

Bu proje, petrol istasyonları sahasındaki otomasyon sistemleri ile merkez CRM altyapısını entegre eden; kurumsal filo limit kontrolü, yakıt türü kısıtlaması ve bireysel müşteri sadakat puanı yönetimini sağlayan **C# .NET 8 RESTful Web API** ve **MSSQL** mimarisidir.

---

## 📐 Mimari ve Teknolojiler

* **Backend:** C# .NET 8 (ASP.NET Core Web API)
* **Veritabanı:** MSSQL Server (Relational DB Design)
* **Dokümantasyon & Test:** OpenAPI 3.0 / Swagger UI
* **Veri Erişim Katmanı:** ADO.NET / Microsoft.Data.SqlClient (Performans Odaklı)
* **Mimari Yapı:** DTO (Data Transfer Objects) & Separation of Concerns

---

## 🎯 Desteklenen İş Kuralları ve Kabul Kriterleri (BRD / FSD v1.0)

### 1. Kurumsal Filo Limit Kontrolü (US-101)
* **AC-101.1:** Yakıt alımı öncesinde aracın bağlı olduğu şirketin ve kendisinin günlük kalan limiti hesaplanır.
* **AC-101.3:** Tutar kalan limiti aşıyorsa işlem `ERR_LIMIT_EXCEEDED` hatasıyla reddedilir ve pompa açılmaz.

### 2. Yakıt Tipi Yetkilendirmesi (US-102)
* **AC-102.1:** Aracın deposuna uygun olmayan yakıt türü (örneğin Benzinli araca Motorin) talep edilirse işlem `ERR_WRONG_FUEL_TYPE` hatasıyla engellenir.

### 3. Bireysel Müşteri Sadakat Puanı (US-201)
* **AC-201.1:** Filo kaydı olmayan bireysel müşterilerin alımlarında her **100 TL** için **1 Puan** hesaplanır.
* **AC-201.2:** Filo araç alımlarında puan kazanımı tetiklenmez.
* **AC-201.3:** Onaylanan puandan sonra veritabanındaki müşteri bakiyesi (`Customers.TotalPoints`) anlık güncellenir.

---

## 🔌 API Endpoint Spesifikasyonu

### `POST /api/v1/pumps/authorization-check`

#### Örnek İstek Gövdesi (Request Body):
```json
{
  "stationId": 101,
  "pumpNumber": 4,
  "licensePlate": "06ANK999",
  "requestedFuelType": "DIESEL",
  "estimatedAmountTL": 500.00,
  "customerPhoneNumber": "+905551112233"
}
Örnek Başarılı Yanıt (Response Body - 200 OK):
{
  "isAuthorized": true,
  "responseCode": "AUTH_SUCCESS",
  "message": "Pompa açılabilir. Limit uygun.",
  "data": {
    "companyName": "Ekinci Lojistik A.Ş.",
    "dailyRemainingLimitTL": 1050.00,
    "allowedFuelType": "DIESEL",
    "earnedPoints": 0.0,
    "totalPoints": 0.0
  }
}
🗄️ Veritabanı Şeması (MSSQL)
Sistem aşağıdaki ilişkisel tablolar üzerine kurulmuştur:

Companies (Kurumsal Filo Şirketleri ve Aktiflik Durumları)

Vehicles (Plakalar, Günlük Limitler, Harcanan Tutar, İzin Verilen Yakıt Türü)

Customers (Bireysel Müşteriler ve Sadakat Puan Bakiyeleri)

FuelTransactions (Pompa Satış Logları)

🚀 Projeyi Yerelde Çalıştırma
Veritabanı_Kurulumu.sql dosyasını MSSQL Server Management Studio (SSMS) üzerinde çalıştırarak veritabanını ve örnek verileri oluşturun.

FuelFlex.CRM.Api.sln çözüm dosyasını Visual Studio 2022 ile açın.

PumpsController.cs içerisindeki bağlantı dizinini kendi yerel SQL Server adınıza göre güncelleyin.

F5 tuşuna basarak projeyi başlatın ve Swagger UI üzerinden API testlerini gerçekleştirin.
