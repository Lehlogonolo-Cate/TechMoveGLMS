IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Clients] (
    [ClientId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [ContactDetails] nvarchar(150) NOT NULL,
    [Region] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
);

CREATE TABLE [Contracts] (
    [ContractId] int NOT NULL IDENTITY,
    [ClientId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [ServiceLevel] nvarchar(100) NOT NULL,
    [AgreementFileName] nvarchar(max) NULL,
    [AgreementStoredFileName] nvarchar(max) NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([ContractId]),
    CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE NO ACTION
);

CREATE TABLE [ServiceRequests] (
    [ServiceRequestId] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [CostUsd] decimal(18,2) NOT NULL,
    [ExchangeRate] decimal(18,4) NOT NULL,
    [Cost] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([ServiceRequestId]),
    CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([ContractId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);

CREATE INDEX [IX_Contracts_Status_StartDate_EndDate] ON [Contracts] ([Status], [StartDate], [EndDate]);

CREATE INDEX [IX_ServiceRequests_ContractId] ON [ServiceRequests] ([ContractId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261006090947_InitialCreate', N'10.0.12');

COMMIT;
GO

