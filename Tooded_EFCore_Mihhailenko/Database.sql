-- Recreates the whole Tooded_DB database

IF DB_ID(N'Tooded_DB') IS NULL
    CREATE DATABASE Tooded_DB;
GO

USE Tooded_DB;
GO

IF OBJECT_ID(N'dbo.Kategooriatabel', N'U') IS NULL
CREATE TABLE Kategooriatabel (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    Kategooria_nimetus  NVARCHAR(100) NOT NULL,
    Kirjeldus           NVARCHAR(200) NULL
);
GO

IF OBJECT_ID(N'dbo.Toodetabel', N'U') IS NULL
CREATE TABLE Toodetabel (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Toodenimetus  NVARCHAR(100) NOT NULL,
    Kogus         INT           NOT NULL CHECK (Kogus >= 0),
    Hind          DECIMAL(10,2) NOT NULL CHECK (Hind >= 0),
    Pilt          NVARCHAR(200) NULL,
    KategooriaId  INT           NOT NULL REFERENCES Kategooriatabel(Id)
);
GO

IF OBJECT_ID(N'dbo.Kasutajatabel', N'U') IS NULL
CREATE TABLE Kasutajatabel (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Eesnimi       NVARCHAR(50)  NOT NULL,
    Perenimi      NVARCHAR(50)  NOT NULL,
    Epost         NVARCHAR(100) NOT NULL,
    Kasutajanimi  NVARCHAR(50)  NOT NULL UNIQUE,
    ParoolHash    NVARCHAR(200) NOT NULL,
    Sool          NVARCHAR(100) NOT NULL,
    Roll          NVARCHAR(20)  NOT NULL,
    Loodud        DATETIME2     NOT NULL DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID(N'dbo.Tellimustabel', N'U') IS NULL
CREATE TABLE Tellimustabel (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    KasutajaId  INT           NOT NULL REFERENCES Kasutajatabel(Id),
    Aeg         DATETIME2     NOT NULL,
    Summa       DECIMAL(10,2) NOT NULL
);
GO

IF OBJECT_ID(N'dbo.Tellimusereatabel', N'U') IS NULL
CREATE TABLE Tellimusereatabel (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    TellimusId  INT           NOT NULL REFERENCES Tellimustabel(Id) ON DELETE CASCADE,
    ToodeId     INT           NULL REFERENCES Toodetabel(Id) ON DELETE SET NULL,
    ToodeNimi   NVARCHAR(100) NOT NULL,
    Kogus       INT           NOT NULL CHECK (Kogus > 0),
    Hind        DECIMAL(10,2) NOT NULL
);
GO

-- Sample data, only if the tables are empty
IF NOT EXISTS (SELECT 1 FROM Kategooriatabel)
BEGIN
    INSERT INTO Kategooriatabel (Kategooria_nimetus, Kirjeldus)
    VALUES (N'Joogid', N''), (N'Toidutooted', N''), (N'Lihatooted', N'');
END
GO

IF NOT EXISTS (SELECT 1 FROM Toodetabel)
BEGIN
    INSERT INTO Toodetabel (Toodenimetus, Kogus, Hind, Pilt, KategooriaId)
    SELECT N'Piim',     12, 1.00, N'piim.jpg',     Id FROM Kategooriatabel WHERE Kategooria_nimetus = N'Toidutooted'
    UNION ALL
    SELECT N'Sea liha',  2, 2.00, N'Sea liha.jpg', Id FROM Kategooriatabel WHERE Kategooria_nimetus = N'Lihatooted'
    UNION ALL
    SELECT N'Õun',       1, 1.00, N'Õun.jpg',      Id FROM Kategooriatabel WHERE Kategooria_nimetus = N'Toidutooted';
END
GO

-- Check: should list the 5 tables
SELECT name AS tabel FROM sys.tables ORDER BY name;