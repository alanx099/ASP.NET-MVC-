DO $$
DECLARE
    target_customer_id bigint;
BEGIN
    -- Change this SELECT if you want to seed guitars for a specific customer:
    -- SELECT 1 INTO target_customer_id;
    SELECT "Id"
    INTO target_customer_id
    FROM "Stranke"
    ORDER BY "Id"
    LIMIT 1;

    IF target_customer_id IS NULL THEN
        RAISE EXCEPTION 'Cannot add guitars because table "Stranke" has no customers.';
    END IF;

    INSERT INTO "Gitare" ("SerijskiBroj", "MarkaId", "BrojZica", "TipGitareId", "DatumZaprimanja", "KupacId")
    VALUES
        ('AC-FEN-2026-001', 1, '6', 1, NOW() - INTERVAL '50 days', target_customer_id),
        ('EL-FEN-2026-002', 1, '6', 2, NOW() - INTERVAL '49 days', target_customer_id),
        ('BS-FEN-2026-003', 1, '4', 4, NOW() - INTERVAL '48 days', target_customer_id),
        ('EL-GIB-2026-004', 2, '6', 2, NOW() - INTERVAL '47 days', target_customer_id),
        ('AC-GIB-2026-005', 2, '6', 1, NOW() - INTERVAL '46 days', target_customer_id),
        ('EL-GIB-2026-006', 2, '7', 2, NOW() - INTERVAL '45 days', target_customer_id),
        ('AC-YAM-2026-007', 3, '6', 1, NOW() - INTERVAL '44 days', target_customer_id),
        ('CL-YAM-2026-008', 3, '6', 3, NOW() - INTERVAL '43 days', target_customer_id),
        ('BS-YAM-2026-009', 3, '5', 4, NOW() - INTERVAL '42 days', target_customer_id),
        ('EL-IBA-2026-010', 4, '6', 2, NOW() - INTERVAL '41 days', target_customer_id),
        ('EL-IBA-2026-011', 4, '7', 2, NOW() - INTERVAL '40 days', target_customer_id),
        ('BS-IBA-2026-012', 4, '5', 4, NOW() - INTERVAL '39 days', target_customer_id),
        ('AC-TAY-2026-013', 5, '6', 1, NOW() - INTERVAL '38 days', target_customer_id),
        ('AC-TAY-2026-014', 5, '12', 1, NOW() - INTERVAL '37 days', target_customer_id),
        ('AC-TAY-2026-015', 5, '6', 1, NOW() - INTERVAL '36 days', target_customer_id),
        ('AC-MAR-2026-016', 6, '6', 1, NOW() - INTERVAL '35 days', target_customer_id),
        ('AC-MAR-2026-017', 6, '12', 1, NOW() - INTERVAL '34 days', target_customer_id),
        ('CL-MAR-2026-018', 6, '6', 3, NOW() - INTERVAL '33 days', target_customer_id),
        ('EL-PRS-2026-019', 7, '6', 2, NOW() - INTERVAL '32 days', target_customer_id),
        ('EL-PRS-2026-020', 7, '7', 2, NOW() - INTERVAL '31 days', target_customer_id),
        ('EL-PRS-2026-021', 7, '6', 2, NOW() - INTERVAL '30 days', target_customer_id),
        ('EL-EPI-2026-022', 8, '6', 2, NOW() - INTERVAL '29 days', target_customer_id),
        ('AC-EPI-2026-023', 8, '6', 1, NOW() - INTERVAL '28 days', target_customer_id),
        ('BS-EPI-2026-024', 8, '4', 4, NOW() - INTERVAL '27 days', target_customer_id),
        ('EL-JAC-2026-025', 9, '6', 2, NOW() - INTERVAL '26 days', target_customer_id),
        ('EL-JAC-2026-026', 9, '7', 2, NOW() - INTERVAL '25 days', target_customer_id),
        ('EL-JAC-2026-027', 9, '8', 2, NOW() - INTERVAL '24 days', target_customer_id),
        ('EL-GRE-2026-028', 10, '6', 2, NOW() - INTERVAL '23 days', target_customer_id),
        ('AC-GRE-2026-029', 10, '6', 1, NOW() - INTERVAL '22 days', target_customer_id),
        ('BS-GRE-2026-030', 10, '4', 4, NOW() - INTERVAL '21 days', target_customer_id),
        ('EL-ESP-2026-031', 11, '6', 2, NOW() - INTERVAL '20 days', target_customer_id),
        ('EL-ESP-2026-032', 11, '7', 2, NOW() - INTERVAL '19 days', target_customer_id),
        ('BS-ESP-2026-033', 11, '5', 4, NOW() - INTERVAL '18 days', target_customer_id),
        ('EL-SCH-2026-034', 12, '6', 2, NOW() - INTERVAL '17 days', target_customer_id),
        ('EL-SCH-2026-035', 12, '7', 2, NOW() - INTERVAL '16 days', target_customer_id),
        ('BS-SCH-2026-036', 12, '4', 4, NOW() - INTERVAL '15 days', target_customer_id),
        ('EL-SQU-2026-037', 13, '6', 2, NOW() - INTERVAL '14 days', target_customer_id),
        ('BS-SQU-2026-038', 13, '4', 4, NOW() - INTERVAL '13 days', target_customer_id),
        ('EL-SQU-2026-039', 13, '6', 2, NOW() - INTERVAL '12 days', target_customer_id),
        ('AC-TAK-2026-040', 14, '6', 1, NOW() - INTERVAL '11 days', target_customer_id),
        ('CL-TAK-2026-041', 14, '6', 3, NOW() - INTERVAL '10 days', target_customer_id),
        ('AC-TAK-2026-042', 14, '12', 1, NOW() - INTERVAL '9 days', target_customer_id),
        ('EL-CHA-2026-043', 15, '6', 2, NOW() - INTERVAL '8 days', target_customer_id),
        ('EL-CHA-2026-044', 15, '7', 2, NOW() - INTERVAL '7 days', target_customer_id),
        ('EL-CHA-2026-045', 15, '6', 2, NOW() - INTERVAL '6 days', target_customer_id),
        ('CL-YAM-2026-046', 3, '6', 3, NOW() - INTERVAL '5 days', target_customer_id),
        ('AC-MAR-2026-047', 6, '6', 1, NOW() - INTERVAL '4 days', target_customer_id),
        ('EL-GIB-2026-048', 2, '6', 2, NOW() - INTERVAL '3 days', target_customer_id),
        ('BS-IBA-2026-049', 4, '6', 4, NOW() - INTERVAL '2 days', target_customer_id),
        ('EL-PRS-2026-050', 7, '6', 2, NOW() - INTERVAL '1 day', target_customer_id);
END $$;
