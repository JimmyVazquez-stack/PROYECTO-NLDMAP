CREATE CONSTRAINT reporte_id_unico IF NOT EXISTS
FOR (r:Reporte) REQUIRE r.idReporte IS UNIQUE;

CREATE CONSTRAINT circunstancia_tipo_unico IF NOT EXISTS
FOR (c:Circunstancia) REQUIRE c.tipo IS UNIQUE;
