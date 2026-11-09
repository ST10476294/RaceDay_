IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END
GO

USE RaceDayDB;
GO


IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL DROP TABLE dbo.Routes;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;


CREATE TABLE dbo.Users (
    UserId        INT IDENTITY(1,1) PRIMARY KEY,
    FullName      VARCHAR(100)  NOT NULL,
    Email         VARCHAR(150)  NOT NULL UNIQUE,
    PasswordHash  VARCHAR(255)  NOT NULL,
    Role          VARCHAR(20)   NOT NULL DEFAULT 'Participant'
                  CHECK (Role IN ('Organiser', 'Participant')),
    CreatedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    ProfilePictureUrl VARCHAR(500) NULL
);

CREATE TABLE dbo.Events (
    EventId       INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId   INT           NOT NULL,
    Name          VARCHAR(150)  NOT NULL,
    Discipline    VARCHAR(20)   NOT NULL
                  CHECK (Discipline IN ('Running', 'Walking', 'Cycling')),
    EventDate     DATETIME      NOT NULL,
    Location      VARCHAR(150)  NOT NULL,
    Description   VARCHAR(500)  NULL,
    DistanceKm    DECIMAL(5,2)  NOT NULL DEFAULT 0,
    BannerImageUrl VARCHAR(500) NULL,
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId)
        REFERENCES dbo.Users(UserId)
);


CREATE TABLE dbo.Categories (
    CategoryId      INT IDENTITY(1,1) PRIMARY KEY,
    EventId         INT           NOT NULL,
    Name            VARCHAR(50)   NOT NULL,
    DistanceKm      DECIMAL(5,2)  NOT NULL,
    MaxParticipants INT           NOT NULL DEFAULT 100,
    EntryFee        DECIMAL(8,2)  NOT NULL DEFAULT 0,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId)
);


CREATE TABLE dbo.Routes (
    RouteId         INT IDENTITY(1,1) PRIMARY KEY,
    EventId         INT           NOT NULL,
    Description     VARCHAR(500)  NULL,
    ElevationGainM  INT           NULL,
    MapUrl          VARCHAR(255)  NULL,
    CONSTRAINT FK_Routes_Events FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId)
);


CREATE TABLE dbo.Enrolments (
    EnrolmentId     INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId   INT           NOT NULL,
    CategoryId      INT           NOT NULL,
    EnrolmentDate   DATETIME      NOT NULL DEFAULT GETDATE(),
    Status          VARCHAR(20)   NOT NULL DEFAULT 'Confirmed'
                    CHECK (Status IN ('Confirmed', 'Cancelled', 'Pending')),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId)
        REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantId, CategoryId)
);


CREATE TABLE dbo.Results (
    ResultId          INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId       INT           NOT NULL UNIQUE,
    FinishTime        TIME          NULL,
    OverallPosition   INT           NULL,
    CategoryPosition  INT           NULL,
    Status            VARCHAR(20)   NOT NULL DEFAULT 'Finished'
                      CHECK (Status IN ('Finished', 'DNF', 'DSQ')),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId)
        REFERENCES dbo.Enrolments(EnrolmentId)
);


-- Every seed user's demonstration password is RaceDay!2026 (stored as a BCrypt hash).
INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role) VALUES
('Thabo Nkosi',   'thabo.nkosi@raceday.co.za',   '$2a$11$gnA7IPZp.3Oc1EiMgH.J7.M0v.tmR3l2bLkvGpODT2WqH1bp8mz5y', 'Organiser'),
('Lerato Dube',   'lerato.dube@raceday.co.za',   '$2a$11$gnA7IPZp.3Oc1EiMgH.J7.M0v.tmR3l2bLkvGpODT2WqH1bp8mz5y', 'Organiser'),
('Sipho Mokoena', 'sipho.mokoena@example.com',   '$2a$11$gnA7IPZp.3Oc1EiMgH.J7.M0v.tmR3l2bLkvGpODT2WqH1bp8mz5y', 'Participant'),
('Amahle Zulu',   'amahle.zulu@example.com',     '$2a$11$gnA7IPZp.3Oc1EiMgH.J7.M0v.tmR3l2bLkvGpODT2WqH1bp8mz5y', 'Participant');


INSERT INTO dbo.Events (OrganiserId, Name, Discipline, EventDate, Location, Description, DistanceKm) VALUES
(1, 'Johannesburg City Run', 'Running', '2026-10-10 07:00:00', 'Johannesburg, Gauteng', 'A scenic road run through the city centre.', 21.10),
(1, 'Soweto Cycle Challenge', 'Cycling', '2027-03-07 06:30:00', 'Soweto, Gauteng', 'A community cycling event through historic Soweto.', 80.00),
(2, 'Durban Beachfront Walk', 'Walking', '2027-04-17 08:00:00', 'Durban, KwaZulu-Natal', 'A relaxed charity walk along the Durban beachfront.', 5.00);


INSERT INTO dbo.Categories (EventId, Name, DistanceKm, MaxParticipants, EntryFee) VALUES
(1, '10km',  10.00, 500, 150.00),
(1, '21km',  21.10, 300, 250.00),
(2, '40km',  40.00, 400, 300.00),
(2, '80km',  80.00, 200, 450.00),
(3, '5km',    5.00, 1000, 80.00);


INSERT INTO dbo.Routes (EventId, Description, ElevationGainM, MapUrl) VALUES
(1, 'Loop route starting and ending at Mary Fitzgerald Square.', 120, 'https://maps.example.com/jhb-city-run'),
(2, 'Out-and-back route through Soweto''s main avenues.', 340, 'https://maps.example.com/soweto-cycle'),
(3, 'Flat out-and-back route along the Durban promenade.', 10, 'https://maps.example.com/durban-walk');


INSERT INTO dbo.Enrolments (ParticipantId, CategoryId, Status) VALUES
(3, 1, 'Confirmed'),
(3, 3, 'Confirmed'),
(4, 2, 'Confirmed'),
(4, 5, 'Confirmed');


INSERT INTO dbo.Results (EnrolmentId, FinishTime, OverallPosition, CategoryPosition, Status) VALUES
(1, '00:48:32', 15, 5, 'Finished'),
(3, '01:45:10', 42, 12, 'Finished');