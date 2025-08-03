SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: test_schema; Type: SCHEMA; Schema: -; Owner: shoppinglist
--
DO $$BEGIN
IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'shoppinglist')
THEN CREATE ROLE shoppinglist WITH
	LOGIN
	SUPERUSER
	CREATEDB
	CREATEROLE
	INHERIT
	NOREPLICATION
	CONNECTION LIMIT -1;
END IF;
END$$;


GRANT postgres TO shoppinglist;

CREATE SCHEMA test_schema;


ALTER SCHEMA test_schema OWNER TO shoppinglist;

--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- Name: update_timestamp(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_timestamp() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.changed = now();
    RETURN NEW;   
END;
$$;


ALTER FUNCTION public.update_timestamp() OWNER TO postgres;



--
-- Name: ft_parser; Type: TEXT SEARCH PARSER; Schema: test_schema; Owner: 
--

CREATE TEXT SEARCH PARSER test_schema.ft_parser (
    START = prsd_start,
    GETTOKEN = prsd_nexttoken,
    END = prsd_end,
    HEADLINE = prsd_headline,
    LEXTYPES = prsd_lextype );




--
-- Name: ft_template; Type: TEXT SEARCH TEMPLATE; Schema: test_schema; Owner: 
--

CREATE TEXT SEARCH TEMPLATE test_schema.ft_template (
    INIT = dsimple_init,
    LEXIZE = dsimple_lexize );



--
-- Name: ft_dictionary; Type: TEXT SEARCH DICTIONARY; Schema: test_schema; Owner: postgres
--

CREATE TEXT SEARCH DICTIONARY test_schema.ft_dictionary (
    TEMPLATE = pg_catalog.simple );


ALTER TEXT SEARCH DICTIONARY test_schema.ft_dictionary OWNER TO postgres;



--
-- Name: ft_configuration; Type: TEXT SEARCH CONFIGURATION; Schema: test_schema; Owner: postgres
--

CREATE TEXT SEARCH CONFIGURATION test_schema.ft_configuration (
    PARSER = pg_catalog."default" );


ALTER TEXT SEARCH CONFIGURATION test_schema.ft_configuration OWNER TO postgres;

SET default_tablespace = '';

SET default_with_oids = false;


--
-- Name: contributors; Type: TABLE; Schema: public; Owner: shoppinglist
--

CREATE TABLE public.contributors (
    id integer NOT NULL,
    list_id integer,
    user_id integer,
    permission smallint
);


ALTER TABLE public.contributors OWNER TO shoppinglist;

--
-- Name: contributors_id_seq; Type: SEQUENCE; Schema: public; Owner: shoppinglist
--

CREATE SEQUENCE public.contributors_id_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.contributors_id_seq OWNER TO shoppinglist;

--
-- Name: contributors_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: shoppinglist
--

ALTER SEQUENCE public.contributors_id_seq OWNED BY public.contributors.id;


--
-- Name: list_item; Type: TABLE; Schema: public; Owner: shoppinglist
--

CREATE TABLE public.list_item (
    id integer NOT NULL,
    list_id integer NOT NULL,
    gtin text,
    name text,
    amount integer,
    bought_amount integer,
    created timestamp(4) with time zone DEFAULT now(),
    changed timestamp(4) with time zone,
    sort_order integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.list_item OWNER TO shoppinglist;

--
-- Name: products; Type: TABLE; Schema: public; Owner: shoppinglist
--

CREATE TABLE public.products (
    quantity numeric,
    unit text,
    name text NOT NULL,
    gtin text NOT NULL
);


ALTER TABLE public.products OWNER TO shoppinglist;

--
-- Name: shoppinglists; Type: TABLE; Schema: public; Owner: shoppinglist
--

CREATE TABLE public.shoppinglists (
    id integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE public.shoppinglists OWNER TO shoppinglist;

--
-- Name: shoppinglists_id_seq; Type: SEQUENCE; Schema: public; Owner: shoppinglist
--

CREATE SEQUENCE public.shoppinglists_id_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.shoppinglists_id_seq OWNER TO shoppinglist;

--
-- Name: shoppinglists_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: shoppinglist
--

ALTER SEQUENCE public.shoppinglists_id_seq OWNED BY public.shoppinglists.id;


--
-- Name: sl_products_id_seq; Type: SEQUENCE; Schema: public; Owner: shoppinglist
--

CREATE SEQUENCE public.sl_products_id_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.sl_products_id_seq OWNER TO shoppinglist;

--
-- Name: sl_products_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: shoppinglist
--

ALTER SEQUENCE public.sl_products_id_seq OWNED BY public.list_item.id;


--
-- Name: user_resettoken; Type: TABLE; Schema: public; Owner: shoppinglist
--

CREATE TABLE public.user_resettoken (
    reset_token text NOT NULL,
    user_id integer NOT NULL,
    "timestamp" timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.user_resettoken OWNER TO shoppinglist;

--
-- Name: users; Type: TABLE; Schema: public; Owner: shoppinglist
--

CREATE TABLE public.users (
    id integer NOT NULL,
    username text NOT NULL,
    email text NOT NULL,
    salt bytea NOT NULL,
    password_hash bytea NOT NULL
);


ALTER TABLE public.users OWNER TO shoppinglist;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: shoppinglist
--

CREATE SEQUENCE public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.users_id_seq OWNER TO shoppinglist;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: shoppinglist
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: gtins; Type: TABLE; Schema: test_schema; Owner: shoppinglist
--

CREATE TABLE test_schema.gtins (
    id integer NOT NULL,
    gtin text NOT NULL
);


ALTER TABLE test_schema.gtins OWNER TO shoppinglist;

--
-- Name: gtins_id_seq; Type: SEQUENCE; Schema: test_schema; Owner: shoppinglist
--

CREATE SEQUENCE test_schema.gtins_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE test_schema.gtins_id_seq OWNER TO shoppinglist;

--
-- Name: gtins_id_seq; Type: SEQUENCE OWNED BY; Schema: test_schema; Owner: shoppinglist
--

ALTER SEQUENCE test_schema.gtins_id_seq OWNED BY test_schema.gtins.id;


--
-- Name: products; Type: TABLE; Schema: test_schema; Owner: shoppinglist
--

CREATE TABLE test_schema.products (
    id integer NOT NULL,
    provider_key smallint NOT NULL,
    quantity numeric,
    unit text,
    name text NOT NULL,
    external_id text,
    fitness smallint
);


ALTER TABLE test_schema.products OWNER TO shoppinglist;

--
-- Name: products_gtins; Type: TABLE; Schema: test_schema; Owner: shoppinglist
--

CREATE TABLE test_schema.products_gtins (
    gtin_id integer NOT NULL,
    product_id integer NOT NULL
);


ALTER TABLE test_schema.products_gtins OWNER TO shoppinglist;

--
-- Name: products_id_seq; Type: SEQUENCE; Schema: test_schema; Owner: shoppinglist
--

CREATE SEQUENCE test_schema.products_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE test_schema.products_id_seq OWNER TO shoppinglist;

--
-- Name: products_id_seq; Type: SEQUENCE OWNED BY; Schema: test_schema; Owner: shoppinglist
--

ALTER SEQUENCE test_schema.products_id_seq OWNED BY test_schema.products.id;



--
-- Name: contributors id; Type: DEFAULT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.contributors ALTER COLUMN id SET DEFAULT nextval('public.contributors_id_seq'::regclass);


--
-- Name: list_item id; Type: DEFAULT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.list_item ALTER COLUMN id SET DEFAULT nextval('public.sl_products_id_seq'::regclass);


--
-- Name: shoppinglists id; Type: DEFAULT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.shoppinglists ALTER COLUMN id SET DEFAULT nextval('public.shoppinglists_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Name: gtins id; Type: DEFAULT; Schema: test_schema; Owner: shoppinglist
--

ALTER TABLE ONLY test_schema.gtins ALTER COLUMN id SET DEFAULT nextval('test_schema.gtins_id_seq'::regclass);


--
-- Name: products id; Type: DEFAULT; Schema: test_schema; Owner: shoppinglist
--

ALTER TABLE ONLY test_schema.products ALTER COLUMN id SET DEFAULT nextval('test_schema.products_id_seq'::regclass);


--
-- Name: users ID; Type: CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT "ID" UNIQUE (id);


--
-- Name: list_item Id; Type: CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.list_item
    ADD CONSTRAINT "Id" PRIMARY KEY (id);


--
-- Name: contributors cID; Type: CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.contributors
    ADD CONSTRAINT "cID" PRIMARY KEY (id);

    
--
-- Name: products products_pkey; Type: CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (gtin);


--
-- Name: shoppinglists unique_id; Type: CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.shoppinglists
    ADD CONSTRAINT unique_id PRIMARY KEY (id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: gtins gtins_pkey; Type: CONSTRAINT; Schema: test_schema; Owner: shoppinglist
--

ALTER TABLE ONLY test_schema.gtins
    ADD CONSTRAINT gtins_pkey PRIMARY KEY (id);


--
-- Name: products_gtins products_gtins_pkey; Type: CONSTRAINT; Schema: test_schema; Owner: shoppinglist
--

ALTER TABLE ONLY test_schema.products_gtins
    ADD CONSTRAINT products_gtins_pkey PRIMARY KEY (product_id, gtin_id);


--
-- Name: products products_pkey; Type: CONSTRAINT; Schema: test_schema; Owner: shoppinglist
--

ALTER TABLE ONLY test_schema.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (id);


    --
-- Name: fki_list_Id; Type: INDEX; Schema: public; Owner: shoppinglist
--

CREATE INDEX "fki_list_Id" ON public.contributors USING btree (list_id);


--
-- Name: fki_list_id; Type: INDEX; Schema: public; Owner: shoppinglist
--

CREATE INDEX fki_list_id ON public.list_item USING btree (list_id);


--
-- Name: fki_user_Id; Type: INDEX; Schema: public; Owner: shoppinglist
--

CREATE INDEX "fki_user_Id" ON public.contributors USING btree (user_id);


--
-- Name: fki_user_id; Type: INDEX; Schema: public; Owner: shoppinglist
--

CREATE INDEX fki_user_id ON public.user_resettoken USING btree (user_id);


--
-- Name: ft_product_name; Type: INDEX; Schema: public; Owner: shoppinglist
--

CREATE INDEX ft_product_name ON public.products USING gin (to_tsvector('german'::regconfig, name));


--
-- Name: index; Type: INDEX; Schema: public; Owner: shoppinglist
--

CREATE INDEX index ON public.products USING btree (gtin);


--
-- Name: uproducts_lower_name; Type: INDEX; Schema: public; Owner: shoppinglist
--

CREATE INDEX uproducts_lower_name ON public.products USING btree (lower(name));


--
-- Name: users_username_idx; Type: INDEX; Schema: public; Owner: shoppinglist
--

CREATE UNIQUE INDEX users_username_idx ON public.users USING btree (username);


--
-- Name: gtins_gtin_idx; Type: INDEX; Schema: test_schema; Owner: shoppinglist
--

CREATE INDEX gtins_gtin_idx ON test_schema.gtins USING btree (gtin);


--
-- Name: products_gtins_gtin_id_idx; Type: INDEX; Schema: test_schema; Owner: shoppinglist
--

CREATE INDEX products_gtins_gtin_id_idx ON test_schema.products_gtins USING btree (gtin_id);


--
-- Name: products_lower_name; Type: INDEX; Schema: test_schema; Owner: shoppinglist
--

CREATE INDEX products_lower_name ON test_schema.products USING btree (lower(name));


--
-- Name: products_provider_key_external_id_idx; Type: INDEX; Schema: test_schema; Owner: shoppinglist
--

CREATE INDEX products_provider_key_external_id_idx ON test_schema.products USING btree (provider_key, external_id);


--
-- Name: products_to_tsvector_idx; Type: INDEX; Schema: test_schema; Owner: shoppinglist
--

CREATE INDEX products_to_tsvector_idx ON test_schema.products USING gin (to_tsvector('german'::regconfig, name));


--
-- Name: list_item update_list_item_changed; Type: TRIGGER; Schema: public; Owner: shoppinglist
--

CREATE TRIGGER update_list_item_changed BEFORE UPDATE ON public.list_item FOR EACH ROW EXECUTE PROCEDURE public.update_timestamp();


--
-- Name: contributors lnk_contributors_shoppinglists; Type: FK CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.contributors
    ADD CONSTRAINT lnk_contributors_shoppinglists FOREIGN KEY (list_id) REFERENCES public.shoppinglists(id) MATCH FULL ON DELETE CASCADE;


--
-- Name: contributors lnk_contributors_users; Type: FK CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.contributors
    ADD CONSTRAINT lnk_contributors_users FOREIGN KEY (user_id) REFERENCES public.users(id) MATCH FULL ON DELETE CASCADE;


--
-- Name: list_item lnk_sl_products_shoppinglists; Type: FK CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.list_item
    ADD CONSTRAINT lnk_sl_products_shoppinglists FOREIGN KEY (list_id) REFERENCES public.shoppinglists(id) MATCH FULL ON DELETE CASCADE;


--
-- Name: user_resettoken user_id; Type: FK CONSTRAINT; Schema: public; Owner: shoppinglist
--

ALTER TABLE ONLY public.user_resettoken
    ADD CONSTRAINT user_id FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: products_gtins products_gtins_gtin_id_fkey; Type: FK CONSTRAINT; Schema: test_schema; Owner: shoppinglist
--

ALTER TABLE ONLY test_schema.products_gtins
    ADD CONSTRAINT products_gtins_gtin_id_fkey FOREIGN KEY (gtin_id) REFERENCES test_schema.gtins(id);


--
-- Name: products_gtins products_gtins_product_id_fkey; Type: FK CONSTRAINT; Schema: test_schema; Owner: shoppinglist
--

ALTER TABLE ONLY test_schema.products_gtins
    ADD CONSTRAINT products_gtins_product_id_fkey FOREIGN KEY (product_id) REFERENCES test_schema.products(id);
