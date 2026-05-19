-- PostgreSQL 18 Manual Database Setup Script
-- This script sets up the stock records schema and tables for the ingestion service
-- 
-- Security Note: This script uses the default postgres user with password 'postgres'
-- For production, configure proper role-based access control and strong passwords

-- Create the database if it doesn't exist
CREATE DATABASE stock_records 
    OWNER postgres
    ENCODING 'UTF8'
    LOCALE 'en_US.UTF-8';

-- Connect to the database
\c stock_records

-- Create schema for ingestion-related tables
CREATE SCHEMA IF NOT EXISTS ingestion
    AUTHORIZATION postgres;

GRANT USAGE ON SCHEMA ingestion TO postgres;
GRANT CREATE ON SCHEMA ingestion TO postgres;

-- Create the stock_records table
-- Table: ingestion.stock_records
-- Purpose: Stores stock market data ingested from Kafka feed
-- Security: All input is parameterized (via EF Core) - no SQL injection risk
CREATE TABLE ingestion.stock_records (
    id SERIAL PRIMARY KEY,
    
    -- Stock Information (from domain StockRecord)
    symbol VARCHAR(10) NOT NULL
        CONSTRAINT ck_symbol_not_empty CHECK (symbol <> ''),
    name VARCHAR(255) NOT NULL
        CONSTRAINT ck_name_not_empty CHECK (name <> ''),
    
    -- Price Data (decimal precision for financial accuracy)
    open NUMERIC(19, 4) NOT NULL
        CONSTRAINT ck_open_positive CHECK (open >= 0),
    high NUMERIC(19, 4) NOT NULL
        CONSTRAINT ck_high_positive CHECK (high >= 0),
    low NUMERIC(19, 4) NOT NULL
        CONSTRAINT ck_low_positive CHECK (low >= 0),
    close NUMERIC(19, 4) NOT NULL
        CONSTRAINT ck_close_positive CHECK (close >= 0),
    
    -- Volume (number of shares)
    volume BIGINT NOT NULL
        CONSTRAINT ck_volume_positive CHECK (volume >= 0),
    
    -- Change percentage
    change_percent NUMERIC(5, 2) NOT NULL
        CONSTRAINT ck_change_percent_bounds CHECK (change_percent >= -100 AND change_percent <= 1000),
    
    -- Metadata (from Kafka message)
    exchange VARCHAR(20) NOT NULL
        CONSTRAINT ck_exchange_valid CHECK (exchange IN ('FTSE500', 'NYSE', 'NASDAQ')),
    record_date DATE NOT NULL,
    record_time TIME NOT NULL,
    
    -- Audit Trail
    file_name VARCHAR(255) NOT NULL
        CONSTRAINT ck_file_name_not_empty CHECK (file_name <> ''),
    created_at_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC')
)
TABLESPACE pg_default;

-- Add table comment
COMMENT ON TABLE ingestion.stock_records IS 'Stock market data records ingested from Kafka';

-- Add column comments
COMMENT ON COLUMN ingestion.stock_records.id IS 'Auto-generated primary key';
COMMENT ON COLUMN ingestion.stock_records.symbol IS 'Stock ticker symbol (e.g., AAPL, MSFT)';
COMMENT ON COLUMN ingestion.stock_records.name IS 'Company or exchange name';
COMMENT ON COLUMN ingestion.stock_records.open IS 'Opening price for the period';
COMMENT ON COLUMN ingestion.stock_records.high IS 'Highest price for the period';
COMMENT ON COLUMN ingestion.stock_records.low IS 'Lowest price for the period';
COMMENT ON COLUMN ingestion.stock_records.close IS 'Closing price for the period';
COMMENT ON COLUMN ingestion.stock_records.volume IS 'Trading volume (number of shares traded)';
COMMENT ON COLUMN ingestion.stock_records.change_percent IS 'Percentage change from open to close';
COMMENT ON COLUMN ingestion.stock_records.exchange IS 'Market exchange (FTSE500, NYSE, NASDAQ)';
COMMENT ON COLUMN ingestion.stock_records.record_date IS 'Date the record was recorded';
COMMENT ON COLUMN ingestion.stock_records.record_time IS 'Time the record was recorded';
COMMENT ON COLUMN ingestion.stock_records.file_name IS 'Name of the file this record was ingested from (audit trail)';
COMMENT ON COLUMN ingestion.stock_records.created_at_utc IS 'Timestamp when the record was persisted to the database (UTC)';

-- Create indexes for common queries
-- Index on symbol for fast lookups by stock ticker
CREATE INDEX idx_stock_records_symbol 
    ON ingestion.stock_records (symbol)
    TABLESPACE pg_default;
COMMENT ON INDEX idx_stock_records_symbol IS 'Index on Symbol for fast lookups';

-- Index on exchange for market-specific queries
CREATE INDEX idx_stock_records_exchange 
    ON ingestion.stock_records (exchange)
    TABLESPACE pg_default;
COMMENT ON INDEX idx_stock_records_exchange IS 'Index on Exchange for market-specific queries';

-- Composite index on date and exchange for time-range queries
CREATE INDEX idx_stock_records_date_exchange 
    ON ingestion.stock_records (record_date, exchange)
    TABLESPACE pg_default;
COMMENT ON INDEX idx_stock_records_date_exchange IS 'Index on date and exchange for time-range queries';

-- Index on created timestamp for auditing and retention policies
CREATE INDEX idx_stock_records_created 
    ON ingestion.stock_records (created_at_utc DESC)
    TABLESPACE pg_default;
COMMENT ON INDEX idx_stock_records_created IS 'Index on created timestamp for auditing and retention';

-- Create __EFMigrationsHistory table for EF Core migration tracking
CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
    MigrationId VARCHAR(150) NOT NULL PRIMARY KEY,
    ProductVersion VARCHAR(32) NOT NULL
);

-- Insert the initial migration marker
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('20260517000000_InitialCreate', '9.0.0')
ON CONFLICT DO NOTHING;

-- Verify the setup
SELECT 'Database setup complete!' as status;
SELECT * FROM information_schema.tables WHERE table_schema = 'ingestion';

