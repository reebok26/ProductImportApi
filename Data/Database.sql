CREATE TABLE Products (
    SKU VARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(255),
    EAN VARCHAR(50),
    Manufacturer NVARCHAR(255),
    Category NVARCHAR(100),
    ImageUrl NVARCHAR(500),
    NetPurchasePrice DECIMAL(18,2),
    DeliveryCost DECIMAL(18,2),
    LogisticUnit NVARCHAR(100),
    Stock INT
);
