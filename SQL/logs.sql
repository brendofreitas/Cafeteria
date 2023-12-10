CREATE TABLE Logs
(
    Id serial PRIMARY KEY,
    MessageTemplate text,
    Level varchar(128),
    Timestamp timestamp,
    Exception text,
    Properties jsonb
);
