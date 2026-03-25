-- Seed 1000 products with categories into the Products table
-- Run this after applying EF Core migrations

DO $$
DECLARE
    categories TEXT[] := ARRAY['Electronics', 'Clothing', 'Books', 'Home', 'Sports'];
BEGIN
    FOR i IN 1..1000 LOOP
        INSERT INTO "Products" ("Id", "Name", "Description", "Price", "Category")
        VALUES (
            gen_random_uuid(),
            'Product ' || i,
            'Description for product ' || i || '. Sample product for demonstrating HybridCache in ASP.NET Core.',
            ROUND((RANDOM() * 999 + 1)::numeric, 2),
            categories[1 + (i % 5)]
        );
    END LOOP;
END $$;
