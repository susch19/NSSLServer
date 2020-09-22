CREATE TABLE public.dbversion (
    name        varchar(200) CONSTRAINT versionKey PRIMARY KEY,
    version     varchar(20) NOT NULL
);