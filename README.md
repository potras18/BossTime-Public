```
USE [UserDB]
GO

/****** Object:  Table [dbo].[StripeEvents]    Script Date: 31/08/2025 17:50:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[StripeEvents](
    [EventID] [int] IDENTITY(1,1) NOT NULL,
    [StripeEventId] [varchar](50) NOT NULL,
    [StripePaymentLinkId] [varchar](50) NOT NULL,
    [UserHash] [varchar](128) NOT NULL,
    [UserName] [varchar](50) NOT NULL,
    [PaymentAmount] [int] NOT NULL,
    [CoinAmount] [int] NOT NULL,
    [Completed] [int] NOT NULL,
    [ReqID] [varchar](50) NOT NULL,
    [UniqueKey] [varchar](50) NOT NULL,
    [PackageName] [varchar](50) NULL,
    [PackageID] [varchar](50) NOT NULL,
    [Created] [datetime] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[StripeEvents] ADD  CONSTRAINT [DF_StripeEvents_pCompleted]  DEFAULT ((0)) FOR [Completed]
GO
```
