-- Seed 1000 products into the Products table
-- Run this after applying EF Core migrations

DO $$
BEGIN
    FOR i IN 1..1000 LOOP
        INSERT INTO "Products" ("Id", "Name", "Description", "Price")
        VALUES (
            gen_random_uuid(),
            'Product ' || i,
            'Description for product ' || i || '. This is a sample product for demonstrating in-memory caching in ASP.NET Core.',
            ROUND((RANDOM() * 999 + 1)::numeric, 2)
        );
    END LOOP;
END $$;
