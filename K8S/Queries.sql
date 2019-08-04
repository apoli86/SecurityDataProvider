USE PortfolioSecurity
GO

select pnd.PortfolioId, nd.Date as [NavDate], s.Symbol, s.Name, pnsp.Currency, pnsp.OpenPrice, pnsp.ClosePrice, pnsp.CreateDate as [PortfolioSecurityCreateDate], pnsp.UpdateDate as [SecurityPriceReceivedDate] from [dbo].[PortfolioNavDateSecurityPrice] pnsp
inner join [dbo].[PortfolioNavDate] pnd on pnsp.PortfolioNavDateId = pnd.PortfolioNavDateId
inner join [dbo].[NavDate] nd on pnd.NavDateId = nd.NavDateId
inner join [dbo].[PortfolioSecurity] ps on pnsp.PortfolioSecurityId = ps.PortfolioSecurityId
inner join [dbo].[Security] s on ps.SecurityId = s.SecurityId
where nd.Date = '2019-08-03' AND ps.PortfolioId = 2
order by pnsp.CreateDate desc, s.Symbol

select nd.Date as NavDate, ps.PortfolioId, count(*) as SecuritiesCount, avg(datediff(second, pnsp.CreateDate, pnsp.UpdateDate)) as 'PriceReceivedAvgTimeInSec', min(datediff(second, pnsp.CreateDate, pnsp.UpdateDate)) as 'PriceReceivedMinTimeInSec', max(datediff(second, pnsp.CreateDate, pnsp.UpdateDate)) as 'PriceReceivedMaxTimeInSec', datediff(second, min(pnsp.CreateDate), max(pnsp.UpdateDate)) as [ElapsedTimeInSec] from [dbo].[PortfolioNavDateSecurityPrice] pnsp
inner join [dbo].[PortfolioNavDate] pnd on pnsp.PortfolioNavDateId = pnd.PortfolioNavDateId
inner join [dbo].[NavDate] nd on pnd.NavDateId = nd.NavDateId
inner join [dbo].[PortfolioSecurity] ps on pnsp.PortfolioSecurityId = ps.PortfolioSecurityId
inner join [dbo].[Security] s on ps.SecurityId = s.SecurityId
where nd.Date = '2019-08-03' AND ps.PortfolioId = 2
group by nd.Date, ps.PortfolioId



--delete from portfolionavdatesecurityprice
--delete from portfolionavdate
--delete from navdate

-- select * from navdate

-- select * from security

-- insert into PortfolioSecurity
-- (PortfolioId, SecurityId, CreateDate)
-- select 1 as [PortfolioId], SecurityId, getdate()
-- from Security where Symbol = 'BBUS'