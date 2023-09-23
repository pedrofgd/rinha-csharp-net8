CREATE TABLE IF NOT EXISTS public.pessoas (
    id UUID PRIMARY KEY NOT NULL,
    apelido VARCHAR(32) UNIQUE NOT NULL,
    nome VARCHAR(100) NOT NULL,
    nascimento DATE NOT NULL,
    stack TEXT NULL,
    busca_trgm TEXT GENERATED ALWAYS AS (
        nome || apelido || stack
    ) STORED
);

CREATE EXTENSION pg_trgm;
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_busca_pessoas_trgm ON pessoas USING GIST(busca_trgm GIST_TRGM_OPS(SIGLEN=64));
