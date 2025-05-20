CREATE TABLE Products (
    SKU NVARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(255),
    EAN NVARCHAR(50),
    Manufacturer NVARCHAR(255),
    Category NVARCHAR(max),
    ImageUrl NVARCHAR(max)
);

CREATE TABLE Inventory (
    SKU NVARCHAR(50) PRIMARY KEY,
    Qty INT,
    ShippingCost DECIMAL(10,2),
    Unit NVARCHAR(50)
);

CREATE TABLE Prices (
    SKU NVARCHAR(50) PRIMARY KEY,
    NetPrice DECIMAL(18,2)
);
