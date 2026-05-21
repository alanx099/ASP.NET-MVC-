INSERT INTO "Gitare" ("SerijskiBroj", "MarkaId", "BrojZica", "TipGitareId", "DatumZaprimanja", "KupacId")
SELECT
    'BULK-CUST-1-' || LPAD(series.guitar_no::text, 4, '0') AS "SerijskiBroj",
    ((series.guitar_no - 1) % 15) + 1 AS "MarkaId",
    CASE
        WHEN (((series.guitar_no - 1) % 4) + 1) = 4 THEN
            CASE WHEN series.guitar_no % 3 = 0 THEN '5' ELSE '4' END
        WHEN series.guitar_no % 17 = 0 THEN '12'
        WHEN series.guitar_no % 13 = 0 THEN '7'
        ELSE '6'
    END AS "BrojZica",
    ((series.guitar_no - 1) % 4) + 1 AS "TipGitareId",
    NOW() - (series.guitar_no || ' days')::interval AS "DatumZaprimanja",
    1 AS "KupacId"
FROM generate_series(1, 500) AS series(guitar_no)
WHERE EXISTS (
    SELECT 1
    FROM "Stranke"
    WHERE "Id" = 1
)
AND NOT EXISTS (
    SELECT 1
    FROM "Gitare"
    WHERE "SerijskiBroj" = 'BULK-CUST-1-' || LPAD(series.guitar_no::text, 4, '0')
);
