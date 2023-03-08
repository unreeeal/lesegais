create database Lesegais

use Lesegais

CREATE TABLE [dbo].[BuyerSeller] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (255) NOT NULL,
    [INN]  NVARCHAR (255) NULL
);



CREATE TABLE [dbo].[Lesegais] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [DeclarationNumber] NVARCHAR (255) NOT NULL,
    [BuyerId]           INT            NOT NULL,
    [SellerId]          INT            NOT NULL,
    [Date]              DATE           NOT NULL,
    [WoodFromSeller]    FLOAT (53)     NOT NULL,
    [WoodFromBuyer]     FLOAT (53)     NOT NULL
);


GO


GO
CREATE PROCEDURE BuyerSellerInsertOrUpdateGetId @Name nvarchar(255),@INN nvarchar(255)
AS
IF NOT EXISTS(SELECT TOP 1 1 FROM BuyerSeller WHERE inn = @INN and Name=@Name)
BEGIN
  INSERT INTO BuyerSeller([Name],INN)
  VALUES (@Name ,@INN)
 return  SCOPE_IDENTITY()

END
ELSE
BEGIN
 return( SELECT ID FROM BuyerSeller WHERE INN = @INN and Name=@Name)
END

GO


CREATE PROCEDURE InsertOrUpdate @declarationNumber nvarchar(255),@buyerId int, @sellerId int,@date Date,@woodFromSeller float, @woodFromBuyer float
as
MERGE  lesegais WITH (SERIALIZABLE) AS T
USING (VALUES (@declarationNumber,@buyerId,@sellerId,@date,@woodFromSeller, @woodFromBuyer)) AS U (DeclarationNumber, BuyerId,SellerId,Date,WoodFromSeller,WoodFromBuyer )
    ON U.DeclarationNumber = T.DeclarationNumber
WHEN MATCHED THEN 
    UPDATE SET T.BuyerId=U.BuyerId, T.SellerId=U.SellerId,T.Date=U.Date, T.WoodFromSeller=U.WoodFromSeller, T.WoodFromBuyer=U.WoodFromBuyer 
WHEN NOT MATCHED THEN
    INSERT (DeclarationNumber,BuyerId,SellerId,Date,WoodFromSeller,WoodFromBuyer) 
    VALUES (U.DeclarationNumber,U.BuyerId,U.SellerId,U.Date,U.WoodFromSeller,U.WoodFromBuyer);