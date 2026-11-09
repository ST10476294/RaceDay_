# Changes from the Part 1 plan

`api-endpoint-plan.pdf` is kept unchanged as the original Part 1 evidence. This file records every deliberate difference, as the POE requires.

## Endpoint changes

| Method | Route | Description | Role required | Request body | Expected response | Why it was needed |
|---|---|---|---|---|---|---|
| POST | `/api/auth/login` | Logs in and creates a server-side session (the plan said "token"). | None | `{ email, password }` | 200 - user ID, role and name, session cookie set; 401 - invalid credentials | The Part 2 brief requires session management. |
| POST | `/api/auth/logout` | Ends the session. | Any | None | 200 - logged out | Users must be able to end their session. |
| POST | `/api/auth/register` | Now needs `organiserCode` when `role` is `Organiser`. | None | `{ fullName, email, password, role, organiserCode }` | 201 - user ID; 400; 403 - invalid organiser code; 409 | Without it, anyone could become an Organiser by editing a request. |
| GET | `/api/events/mine` | Lists the Organiser's own past and upcoming events with enrolment totals. | Organiser | None | 200 - events with totals; 401; 403 | The dashboard needs totals, and completed events for results entry. |
| GET | `/api/events/{id}/enrolments` | Lists enrolments for an owned event. | Organiser (owner only) | None | 200 - enrolments; 401; 403; 404 | Required by the brief. |
| POST | `/api/events/{id}/banner` | Uploads a banner to Azure Blob Storage. | Organiser (owner only) | multipart `file` | 200 - event with URL; 400 - invalid image; 401; 403; 404; 503 - storage unavailable | Part 3 Azure Blob Storage requirement. |
| POST | `/api/users/{id}/profile-picture` | Uploads a profile picture to Azure Blob Storage. | Any (own record only) | multipart `file` | 200 - profile with URL; 400; 401; 403; 404; 503 | Part 3 Participant profile requirement. |

Event create and update bodies now also include `distanceKm`.

## Database changes

Migration `Part3ImagesAndEventDistance` adds three columns. `ERD.png`, `ERD.dot` and `RaceDay_Schema.sql` were updated to match.

| Table | Column | Why |
|---|---|---|
| Events | `DistanceKm DECIMAL(5,2) NOT NULL` | The brief requires each event to capture a distance. Categories can still offer other distances. |
| Events | `BannerImageUrl VARCHAR(500) NULL` | Stores the Azure URL of the banner. |
| Users | `ProfilePictureUrl VARCHAR(500) NULL` | Stores the Azure URL of the profile picture. |

## SQL script corrections

- Added `GO` after `CREATE DATABASE` and `USE`, because SQL Server cannot `USE` a database in the same batch that creates it.
- Replaced `HASH_PLACEHOLDER_*` with real BCrypt hashes so the seed users can log in.
- Moved two seeded events into 2027 so that upcoming events exist. The Johannesburg event stays in the past so that its seeded results make sense.

## Behaviour decisions

- **Publishing results:** saving a result publishes it immediately. There is no separate draft stage.
- **Routes:** the Routes table stays read-only through the API, as the Part 1 plan has no route management endpoints. Routes appear on the event page.
- **Enrolment status:** new entries are `Confirmed` immediately, as the brief says to record them immediately. `Pending` remains a valid status and has its own amber badge.
