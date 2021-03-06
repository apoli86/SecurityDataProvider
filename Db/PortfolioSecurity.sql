USE [PortfolioSecurity]
GO
ALTER TABLE [dbo].[PortfolioSecurity] DROP CONSTRAINT [FK_PortfolioSecurity_Security1]
GO
ALTER TABLE [dbo].[PortfolioNavDateSecurityPrice] DROP CONSTRAINT [FK_PortfolioNavDateSecurityPriceId_PortfolioSecurity1]
GO
ALTER TABLE [dbo].[PortfolioNavDateSecurityPrice] DROP CONSTRAINT [FK_PortfolioNavDateSecurityPriceId_PortfolioNavDate1]
GO
ALTER TABLE [dbo].[PortfolioNavDate] DROP CONSTRAINT [FK_PortfolioNavDate_Portfolio]
GO
ALTER TABLE [dbo].[PortfolioNavDate] DROP CONSTRAINT [FK_PortfolioNavDate_NavDate]
GO
ALTER TABLE [dbo].[PortfolioSecurity] DROP CONSTRAINT [DF_PortfolioSecurity_CreateDate]
GO
ALTER TABLE [dbo].[Portfolio] DROP CONSTRAINT [DF_Portfolio_CreateDate]
GO
/****** Object:  Table [dbo].[Security]    Script Date: 04/08/2019 14:33:21 ******/
DROP TABLE [dbo].[Security]
GO
/****** Object:  Table [dbo].[PortfolioSecurity]    Script Date: 04/08/2019 14:33:21 ******/
DROP TABLE [dbo].[PortfolioSecurity]
GO
/****** Object:  Table [dbo].[PortfolioNavDateSecurityPrice]    Script Date: 04/08/2019 14:33:21 ******/
DROP TABLE [dbo].[PortfolioNavDateSecurityPrice]
GO
/****** Object:  Table [dbo].[PortfolioNavDate]    Script Date: 04/08/2019 14:33:21 ******/
DROP TABLE [dbo].[PortfolioNavDate]
GO
/****** Object:  Table [dbo].[Portfolio]    Script Date: 04/08/2019 14:33:21 ******/
DROP TABLE [dbo].[Portfolio]
GO
/****** Object:  Table [dbo].[NavDate]    Script Date: 04/08/2019 14:33:21 ******/
DROP TABLE [dbo].[NavDate]
GO
/****** Object:  Table [dbo].[NavDate]    Script Date: 04/08/2019 14:33:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NavDate](
	[NavDateId] [bigint] IDENTITY(1,1) NOT NULL,
	[Date] [date] NOT NULL,
	[RefreshSecurityStaticDataStatus] [varchar](255) NOT NULL,
 CONSTRAINT [PK_NavDate] PRIMARY KEY CLUSTERED 
(
	[NavDateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Portfolio]    Script Date: 04/08/2019 14:33:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Portfolio](
	[PortfolioId] [bigint] IDENTITY(1,1) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Portfolio] PRIMARY KEY CLUSTERED 
(
	[PortfolioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PortfolioNavDate]    Script Date: 04/08/2019 14:33:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PortfolioNavDate](
	[PortfolioNavDateId] [bigint] IDENTITY(1,1) NOT NULL,
	[NavDateId] [bigint] NOT NULL,
	[PortfolioId] [bigint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PortfolioNavDate] PRIMARY KEY CLUSTERED 
(
	[PortfolioNavDateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UX_PortfolioNavDate_NavDateId_PortfolioId] UNIQUE NONCLUSTERED 
(
	[PortfolioNavDateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PortfolioNavDateSecurityPrice]    Script Date: 04/08/2019 14:33:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PortfolioNavDateSecurityPrice](
	[PortfolioNavDateSecurityPriceId] [bigint] IDENTITY(1,1) NOT NULL,
	[PortfolioNavDateId] [bigint] NOT NULL,
	[PortfolioSecurityId] [bigint] NOT NULL,
	[PriceStatus] [varchar](255) NOT NULL,
	[Currency] [varchar](10) NOT NULL,
	[OpenPrice] [decimal](18, 3) NULL,
	[ClosePrice] [decimal](18, 3) NULL,
	[CreateDate] [datetime] NOT NULL,
	[UpdateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PortfolioNavDateSecurityPriceId] PRIMARY KEY CLUSTERED 
(
	[PortfolioNavDateSecurityPriceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PortfolioSecurity]    Script Date: 04/08/2019 14:33:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PortfolioSecurity](
	[PortfolioSecurityId] [bigint] IDENTITY(1,1) NOT NULL,
	[SecurityId] [bigint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[PortfolioId] [bigint] NOT NULL,
 CONSTRAINT [PK_PortfolioSecurity] PRIMARY KEY CLUSTERED 
(
	[PortfolioSecurityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UX_PortfolioSecurity_SecurityId_PortfolioId] UNIQUE NONCLUSTERED 
(
	[SecurityId] ASC,
	[PortfolioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Security]    Script Date: 04/08/2019 14:33:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security](
	[SecurityId] [bigint] IDENTITY(1,1) NOT NULL,
	[Symbol] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Currency] [varchar](10) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[UpdateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Security] PRIMARY KEY CLUSTERED 
(
	[SecurityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Portfolio] ADD  CONSTRAINT [DF_Portfolio_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[PortfolioSecurity] ADD  CONSTRAINT [DF_PortfolioSecurity_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[PortfolioNavDate]  WITH CHECK ADD  CONSTRAINT [FK_PortfolioNavDate_NavDate] FOREIGN KEY([NavDateId])
REFERENCES [dbo].[NavDate] ([NavDateId])
GO
ALTER TABLE [dbo].[PortfolioNavDate] CHECK CONSTRAINT [FK_PortfolioNavDate_NavDate]
GO
ALTER TABLE [dbo].[PortfolioNavDate]  WITH CHECK ADD  CONSTRAINT [FK_PortfolioNavDate_Portfolio] FOREIGN KEY([PortfolioId])
REFERENCES [dbo].[Portfolio] ([PortfolioId])
GO
ALTER TABLE [dbo].[PortfolioNavDate] CHECK CONSTRAINT [FK_PortfolioNavDate_Portfolio]
GO
ALTER TABLE [dbo].[PortfolioNavDateSecurityPrice]  WITH CHECK ADD  CONSTRAINT [FK_PortfolioNavDateSecurityPriceId_PortfolioNavDate1] FOREIGN KEY([PortfolioNavDateId])
REFERENCES [dbo].[PortfolioNavDate] ([PortfolioNavDateId])
GO
ALTER TABLE [dbo].[PortfolioNavDateSecurityPrice] CHECK CONSTRAINT [FK_PortfolioNavDateSecurityPriceId_PortfolioNavDate1]
GO
ALTER TABLE [dbo].[PortfolioNavDateSecurityPrice]  WITH CHECK ADD  CONSTRAINT [FK_PortfolioNavDateSecurityPriceId_PortfolioSecurity1] FOREIGN KEY([PortfolioSecurityId])
REFERENCES [dbo].[PortfolioSecurity] ([PortfolioSecurityId])
GO
ALTER TABLE [dbo].[PortfolioNavDateSecurityPrice] CHECK CONSTRAINT [FK_PortfolioNavDateSecurityPriceId_PortfolioSecurity1]
GO
ALTER TABLE [dbo].[PortfolioSecurity]  WITH CHECK ADD  CONSTRAINT [FK_PortfolioSecurity_Security1] FOREIGN KEY([SecurityId])
REFERENCES [dbo].[Security] ([SecurityId])
GO
ALTER TABLE [dbo].[PortfolioSecurity] CHECK CONSTRAINT [FK_PortfolioSecurity_Security1]
GO
