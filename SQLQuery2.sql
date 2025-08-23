-- =====================================
-- iServiceSeeker Database Creation Script
-- Run this in SQL Server Management Studio or Visual Studio SQL Server Object Explorer
-- =====================================

-- Create Database
CREATE DATABASE iServiceSeekerDb;
GO

USE iServiceSeekerDb;
GO

-- =====================================
-- ASP.NET Core Identity Tables
-- =====================================

-- AspNetRoles
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

-- AspNetUsers (with custom columns)
CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [UserType] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
    [LastLoginAt] datetime2 NULL,
    [IsProfileComplete] bit NOT NULL DEFAULT(0),
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO
CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

-- AspNetUserRoles
CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

-- AspNetUserClaims
CREATE TABLE [AspNetUserClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

-- AspNetUserLogins
CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

-- AspNetUserTokens
CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetRoleClaims
CREATE TABLE [AspNetRoleClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

-- =====================================
-- Custom Application Tables
-- =====================================

-- ServiceCategories
CREATE TABLE [ServiceCategories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT(1),
    CONSTRAINT [PK_ServiceCategories] PRIMARY KEY ([Id])
);
GO

-- HomeownerProfiles
CREATE TABLE [HomeownerProfiles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Address] nvarchar(500) NULL,
    [City] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    [ZipCode] nvarchar(max) NULL,
    [ReceiveEmailNotifications] bit NOT NULL DEFAULT(1),
    [ReceiveSmsNotifications] bit NOT NULL DEFAULT(0),
    CONSTRAINT [PK_HomeownerProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HomeownerProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_HomeownerProfiles_UserId] ON [HomeownerProfiles] ([UserId]);
GO

-- ContractorProfiles
CREATE TABLE [ContractorProfiles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [CompanyName] nvarchar(200) NOT NULL,
    [LicenseNumber] nvarchar(50) NULL,
    [BusinessAddress] nvarchar(500) NULL,
    [City] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    [ZipCode] nvarchar(max) NULL,
    [IsVerified] bit NOT NULL DEFAULT(0),
    [VerifiedAt] datetime2 NULL,
    [ServiceRadius] decimal(18,2) NOT NULL DEFAULT(25),
    [Description] nvarchar(1000) NULL,
    [Website] nvarchar(max) NULL,
    CONSTRAINT [PK_ContractorProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractorProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_ContractorProfiles_UserId] ON [ContractorProfiles] ([UserId]);
GO

-- ContractorServiceAreas
CREATE TABLE [ContractorServiceAreas] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ContractorProfileId] int NOT NULL,
    [ServiceCategoryId] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT(1),
    CONSTRAINT [PK_ContractorServiceAreas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractorServiceAreas_ContractorProfiles_ContractorProfileId] FOREIGN KEY ([ContractorProfileId]) REFERENCES [ContractorProfiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContractorServiceAreas_ServiceCategories_ServiceCategoryId] FOREIGN KEY ([ServiceCategoryId]) REFERENCES [ServiceCategories] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ContractorServiceAreas_ContractorProfileId] ON [ContractorServiceAreas] ([ContractorProfileId]);
GO
CREATE INDEX [IX_ContractorServiceAreas_ServiceCategoryId] ON [ContractorServiceAreas] ([ServiceCategoryId]);
GO

-- =====================================
-- Insert Default Data
-- =====================================

-- Insert Roles
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES 
('1', 'Admin', 'ADMIN', NEWID()),
('2', 'Homeowner', 'HOMEOWNER', NEWID()),
('3', 'Contractor', 'CONTRACTOR', NEWID());
GO

-- Insert Service Categories
INSERT INTO [ServiceCategories] ([Name], [Description], [IsActive]) VALUES
('General Contracting', 'General construction and renovation services', 1),
('Plumbing', 'Plumbing installation, repair, and maintenance', 1),
('Electrical', 'Electrical installation, repair, and maintenance', 1),
('HVAC', 'Heating, ventilation, and air conditioning services', 1),
('Roofing', 'Roof installation, repair, and maintenance', 1),
('Flooring', 'Floor installation and refinishing', 1),
('Painting', 'Interior and exterior painting services', 1),
('Landscaping', 'Landscape design and maintenance', 1);
GO

-- =====================================
-- Create __EFMigrationsHistory table (to keep EF happy)
-- =====================================
CREATE TABLE [__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);
GO

-- Insert a fake migration record
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES 
('20241201000000_InitialCreate', '9.0.0');
GO

PRINT 'Database created successfully!'
PRINT 'Tables created:'
PRINT '- All ASP.NET Core Identity tables'
PRINT '- HomeownerProfiles'
PRINT '- ContractorProfiles' 
PRINT '- ServiceCategories (with 8 categories)'
PRINT '- ContractorServiceAreas'
PRINT ''
PRINT 'Default roles created: Admin, Homeowner, Contractor'
PRINT 'Ready to run your application!'