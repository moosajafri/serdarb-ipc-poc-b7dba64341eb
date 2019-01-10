CREATE TABLE IF NOT EXISTS public.file
(
  Id				serial							not null primary key,
  Guid				character(32)					not null,
  FileName			character varying(100)			not null,
  Path				character varying(1000)			null,     
  CreatedAt			timestamp without time zone		not null,
  CustomerId		int								not null
);  