BEGIN;

SET search_path TO guitar_service, public;

-- =========================
-- Seed data from GuitarServiceMockData.cs
-- =========================

-- Office
INSERT INTO offices (id, name, address)
VALUES (1, 'Servis Centar Zagreb', 'Radnicka cesta 50, Zagreb')
ON CONFLICT (id) DO NOTHING;

-- Customers
INSERT INTO customers (
    id,
    office_id,
    first_name,
    last_name,
    email,
    phone,
    address,
    registration_date,
    note
) VALUES
(1, 1, 'Marko', 'Markovic', 'marko@email.com', '0953333333', 'Dubrava 1, Zagreb', '2026-03-01', 'Redovan kupac'),
(2, 1, 'Petra', 'Petric', 'petra@email.com', '0964444444', 'Maksimir 2, Zagreb', '2026-03-05', 'Zeli brzi servis'),
(3, 1, 'Tomislav', 'Kovac', 'tomislav.kovac@email.com', '0975555555', 'Tresnjevka 8, Zagreb', '2026-03-12', 'Vlasnik vintage instrumenata')
ON CONFLICT (id) DO NOTHING;

-- Employees (technicians + managers)
INSERT INTO employees (
    id,
    office_id,
    first_name,
    last_name,
    email,
    phone,
    address,
    employment_date,
    salary
) VALUES
(1, 1, 'Ivan', 'Ivic', 'ivan.ivic@servis.hr', '0911111111', 'Ilica 10, Zagreb', '2024-01-15', 1800.00),
(2, 1, 'Lea', 'Lasic', 'lea.lasic@servis.hr', '0922222222', 'Savska 20, Zagreb', '2023-09-01', 1850.00),
(10, 1, 'Ana', 'Anic', 'ana.anic@servis.hr', '0933333333', 'Radnicka 50, Zagreb', '2023-05-10', 2200.00),
(11, 1, 'Marko', 'Maric', 'marko.maric@servis.hr', '0944444444', 'Radnicka 50, Zagreb', '2022-11-01', 2400.00)
ON CONFLICT (id) DO NOTHING;

-- Employee roles
INSERT INTO technicians (id)
VALUES (1), (2)
ON CONFLICT (id) DO NOTHING;

INSERT INTO managers (id)
VALUES (10), (11)
ON CONFLICT (id) DO NOTHING;

-- Guitars
-- brand_id mapping:
-- Fender=1, Gibson=2, Yamaha=3, Taylor=5
-- guitar_type_id mapping:
-- Aukusticna=1, Elektricna=2
INSERT INTO guitars (
    id,
    customer_id,
    serial_number,
    brand_id,
    strings_count,
    guitar_type_id,
    received_at
) VALUES
(1, 1, 'SN001', 1, 6, 2, '2026-03-22 00:00:00'),
(2, 2, 'SN002', 3, 6, 1, '2026-03-27 00:00:00'),
(3, 3, 'SN003', 2, 6, 2, '2026-04-01 00:00:00'),
(4, 1, 'SN004', 5, 12, 1, '2026-04-04 00:00:00')
ON CONFLICT (id) DO NOTHING;

-- Technician skills (Znanje)
-- repair_type_id mapping:
-- ZamjenaZica=1, PodesavanjeVrata=2, PodesavanjeIntonacije=3,
-- ZamjenaPragova=4, PopravakElektronike=6, ZamjenaMasinica=8
INSERT INTO technician_skills (technician_id, guitar_type_id, repair_type_id)
VALUES
(1, 2, 1),
(1, 1, 2),
(1, 2, 6),
(2, 3, 3),
(2, 4, 8)
ON CONFLICT (technician_id, guitar_type_id, repair_type_id) DO NOTHING;

-- Repair orders (Nalog)
-- status_id mapping:
-- Zaprimljen=1, UObradi=2, CekaDijelove=3, Zavrsen=4
INSERT INTO repair_orders (
    id,
    office_id,
    guitar_id,
    customer_id,
    technician_id,
    description,
    opened_at,
    closed_at,
    status_id,
    repair_type_id
) VALUES
(1, 1, 1, 1, 1, 'Pukla zica', '2026-04-08 00:00:00', '2026-04-12 00:00:00', 1, 1),
(2, 1, 2, 2, 1, 'Visok action', '2026-04-10 00:00:00', '2026-04-14 00:00:00', 2, 2),
(3, 1, 3, 3, 2, 'Neispravna elektronika', '2026-04-11 00:00:00', '2026-04-18 00:00:00', 3, 6),
(4, 1, 4, 1, 2, 'Zamjena pragova', '2026-04-13 00:00:00', '2026-04-20 00:00:00', 4, 4)
ON CONFLICT (id) DO NOTHING;

-- Keep sequences in sync after explicit IDs
SELECT setval(pg_get_serial_sequence('offices', 'id'), COALESCE((SELECT MAX(id) FROM offices), 1), true);
SELECT setval(pg_get_serial_sequence('customers', 'id'), COALESCE((SELECT MAX(id) FROM customers), 1), true);
SELECT setval(pg_get_serial_sequence('employees', 'id'), COALESCE((SELECT MAX(id) FROM employees), 1), true);
SELECT setval(pg_get_serial_sequence('guitars', 'id'), COALESCE((SELECT MAX(id) FROM guitars), 1), true);
SELECT setval(pg_get_serial_sequence('repair_orders', 'id'), COALESCE((SELECT MAX(id) FROM repair_orders), 1), true);

COMMIT;
