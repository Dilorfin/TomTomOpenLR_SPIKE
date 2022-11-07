SELECT TOP (1000) [Url], MAX(GotCount), MIN(GotCount)
	FROM [tomtomopenlr-spike].[dbo].[Locations]
	GROUP BY [Url]

SELECT TOP (1000) [Url], [GotCount], COUNT(*) AS 'Locations Count'
	FROM [tomtomopenlr-spike].[dbo].[Locations]
	GROUP BY [Url], [GotCount]
	ORDER BY [Url]


SELECT /*[T1].[Url],*/ [Country], FloatingLocationsCount as ChangedLocationsCount, MaxLocations as StaticLocationsCount, [T1].[%]
FROM [tomtomopenlr-spike].[dbo].[Counties] AS Countries
JOIN
	(SELECT [Temp1].[Url], FloatingLocationsCount, MaxLocations, 100.0 * CAST(FloatingLocationsCount AS float)/CAST(MaxLocations AS float) AS '%' FROM
		(
			SELECT [TT].[Url], SUM(LocationsCount) AS FloatingLocationsCount
			FROM (
					SELECT [Url], [GotCount], COUNT(*) AS LocationsCount
						FROM [tomtomopenlr-spike].[dbo].[Locations]
						GROUP BY [Url], [GotCount]
					) AS TempTable
				JOIN (
					SELECT [Url], MAX(LocationsCount) AS MaxLocations
					FROM (
						SELECT [Url], [GotCount], COUNT(*) AS LocationsCount
							FROM [tomtomopenlr-spike].[dbo].[Locations]
							GROUP BY [Url], [GotCount]
						) AS TempTable
					GROUP BY [Url]
				) AS TT ON TT.Url = TempTable.Url
			WHERE LocationsCount <> MaxLocations
			GROUP BY [TT].[Url]
		) AS Temp1
		JOIN (
			SELECT [Url], MAX(LocationsCount) AS MaxLocations
			FROM (
				SELECT [Url], [GotCount], COUNT(*) AS LocationsCount
					FROM [tomtomopenlr-spike].[dbo].[Locations]
					GROUP BY [Url], [GotCount]
				) AS TempTable
			GROUP BY [Url]
		) AS Temp2
	ON [Temp1].[Url] = [Temp2].[Url]) AS T1
ON [T1].[Url] = [Countries].[Url]

SELECT TOP (1000) *
	FROM [tomtomopenlr-spike].[dbo].Locations
	WHERE GotCount > 1

SELECT *
	FROM [tomtomopenlr-spike].[dbo].Locations
	WHERE 
	/*[Url] = 'https://cert-traffic.tomtom.com/tsq/hdf-detailed/DEU-HDF_DETAILED-OPENLR/a9d1d2ee-2da2-41a5-a943-f28eb4467e3c/content.proto'*/

	/*AND GotCount = 17*/
	/*C9Z2nudrghZNEAJo/2ETGg==*/
	/* C9Z2nudrghZNFwP1/rgMGg== */
	[Location] LIKE 'C9Z2n%'
	ORDER BY [Url]
SELECT
	*
FROM
	[tomtomopenlr-spike].[dbo].Locations
WHERE
	[Location] = 'C9Z2nudrghZNFwP1/rgMGg=='