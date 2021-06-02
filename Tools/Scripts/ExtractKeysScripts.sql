--Unique key uploads per day
SELECT DATEADD(HOUR,-1, CreatedOn) AS 'CreatedOn',      
       OriginId AS 'CountryOfOriginId',
       COUNT(KeyData) AS 'Total'
FROM [xxxx] --ADD TEK TABLE NAME
GROUP BY CreatedOn, OriginId
HAVING 
OriginId = # add country code ID
ORDER BY CreatedOn

--Total per country
SELECT CONVERT(VARCHAR(10), CreatedOn, 112) AS 'CreatedOn_Day', 
       k.OriginId AS 'CountryOfOriginId', 
       (SELECT t.Value FROM Translation t WHERE t.ID = k.OriginId) AS 'Country', 
       COUNT(k.ID) AS 'Total'
FROM [xxxx] k --ADD TEK TABLE NAME
GROUP BY OriginId,  
         CONVERT(VARCHAR(10), CreatedOn, 112) 
ORDER BY CONVERT(VARCHAR(10), CreatedOn, 112),
         OriginId
 