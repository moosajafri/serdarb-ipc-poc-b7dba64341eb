CREATE TABLE IF NOT EXISTS public.customer
(
  Id				serial							not null primary key,
  Guid				character(32)					not null,
  
  Name				character varying(100)			not null,
  Email				character varying(255)			not null,
  Phone				character varying(10)			not null,
  BornAt			timestamp without time zone		not null, 
  CreatedAt			timestamp without time zone		not null 
);  