CREATE TABLE public.pessoas (
    id UUID NOT NULL,
    PRIMARY KEY(id),
    nome VARCHAR(100),
    apelido VARCHAR(32),
    UNIQUE(apelido),
    nascimento date,
    stack text
    );
