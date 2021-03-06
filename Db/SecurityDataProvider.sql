USE [SecurityDataProvider]
GO
ALTER TABLE [dbo].[SecurityPrice] DROP CONSTRAINT [FK_SecurityPrice_Security]
GO
ALTER TABLE [dbo].[Request] DROP CONSTRAINT [DF_Request_CreateDate]
GO
/****** Object:  Table [dbo].[SecurityPrice]    Script Date: 04/08/2019 14:32:13 ******/
DROP TABLE [dbo].[SecurityPrice]
GO
/****** Object:  Table [dbo].[Security]    Script Date: 04/08/2019 14:32:13 ******/
DROP TABLE [dbo].[Security]
GO
/****** Object:  Table [dbo].[Request]    Script Date: 04/08/2019 14:32:13 ******/
DROP TABLE [dbo].[Request]
GO
/****** Object:  Table [dbo].[Request]    Script Date: 04/08/2019 14:32:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Request](
	[RequestId] [bigint] IDENTITY(1,1) NOT NULL,
	[RequestType] [varchar](255) NOT NULL,
	[RequestPayload] [varchar](512) NOT NULL,
	[RequestDate] [date] NOT NULL,
	[RequestStatus] [varchar](255) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[UpdateDate] [datetime] NULL,
 CONSTRAINT [PK_Request] PRIMARY KEY CLUSTERED 
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UX_Request_RequestTypeId_RequestPayload_RequestDate] UNIQUE NONCLUSTERED 
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Security]    Script Date: 04/08/2019 14:32:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security](
	[SecurityId] [bigint] IDENTITY(1,1) NOT NULL,
	[Symbol] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[RequestDate] [date] NOT NULL,
	[Currency] [varchar](10) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Security] PRIMARY KEY CLUSTERED 
(
	[SecurityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SecurityPrice]    Script Date: 04/08/2019 14:32:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SecurityPrice](
	[SecurityPriceId] [bigint] IDENTITY(1,1) NOT NULL,
	[SecurityId] [bigint] NOT NULL,
	[NavDate] [date] NOT NULL,
	[OpenPrice] [decimal](18, 3) NOT NULL,
	[ClosePrice] [decimal](18, 3) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SecurityPrice] PRIMARY KEY CLUSTERED 
(
	[SecurityPriceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UX_SecurityPrice_SecurityId_NavDate] UNIQUE NONCLUSTERED 
(
	[SecurityId] ASC,
	[NavDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Request] ADD  CONSTRAINT [DF_Request_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[SecurityPrice]  WITH CHECK ADD  CONSTRAINT [FK_SecurityPrice_Security] FOREIGN KEY([SecurityId])
REFERENCES [dbo].[Security] ([SecurityId])
GO
ALTER TABLE [dbo].[SecurityPrice] CHECK CONSTRAINT [FK_SecurityPrice_Security]
GO
