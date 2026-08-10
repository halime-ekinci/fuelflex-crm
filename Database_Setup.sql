USE FuelFlexCRM;
GO

-- 1. Companies (Kurumsal Þirketler) Tablosu
CREATE TABLE Companies (
    CompanyID INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(150) NOT NULL,
    TaxNumber VARCHAR(11) NOT NULL UNIQUE,
    TaxOffice NVARCHAR(100) NULL,
    CreditLimitTL DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 2. Vehicles (Filo Araçlarý ve Limit Kurallarý) Tablosu
CREATE TABLE Vehicles (
    VehicleID INT IDENTITY(1,1) PRIMARY KEY,
    CompanyID INT NOT NULL,
    LicensePlate VARCHAR(20) NOT NULL UNIQUE,
    AllowedFuelType VARCHAR(20) NOT NULL,
    DailyLimitTL DECIMAL(18,2) NOT NULL DEFAULT 1000.00,
    DailyUsedTL DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    IsBlocked BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Vehicles_Companies FOREIGN KEY (CompanyID) REFERENCES Companies(CompanyID)
);
GO

-- 3. Customers (Bireysel Sadakat Müþterileri) Tablosu
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    PhoneNumber VARCHAR(15) NOT NULL UNIQUE,
    Email VARCHAR(100) NULL,
    TotalPoints DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 4. FuelTransactions (Saha Satýþ Loglarý) Tablosu
CREATE TABLE FuelTransactions (
    TransactionID BIGINT IDENTITY(1,1) PRIMARY KEY,
    StationID INT NOT NULL,
    PumpNumber INT NOT NULL,
    VehicleID INT NULL,
    CustomerID INT NULL,
    FuelType VARCHAR(20) NOT NULL,
    UnitPriceTL DECIMAL(18,4) NOT NULL,
    Liters DECIMAL(18,2) NOT NULL,
    TotalAmountTL DECIMAL(18,2) NOT NULL,
    EarnedPoints DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Status VARCHAR(20) NOT NULL,
    TransactionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Transactions_Vehicles FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID),
    CONSTRAINT FK_Transactions_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);
GO

-- 5. Test Verilerini Ekleme
INSERT INTO Companies (CompanyName, TaxNumber, CreditLimitTL, CurrentBalance, IsActive)
VALUES ('Ekinci Lojistik A.Þ.', '1234567890', 500000.00, 145250.00, 1);

INSERT INTO Vehicles (CompanyID, LicensePlate, AllowedFuelType, DailyLimitTL, DailyUsedTL, IsBlocked)
VALUES 
(1, '06ANK999', 'DIESEL', 2000.00, 450.00, 0),
(1, '34CDE123', 'GASOLINE', 1000.00, 0.00, 0),
(1, '01EKN010', 'DIESEL', 5000.00, 5000.00, 0);

INSERT INTO Customers (FirstName, LastName, PhoneNumber, TotalPoints)
VALUES ('Halime', 'Ekinci', '+905551112233', 125.50);
GO