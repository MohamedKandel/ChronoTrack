# ChronoTrack — Technical Functional Specification

**Document Type:** Functional Specification
**Status:** Draft for Development
**Audience:** Engineering, QA, Product

---

## 1. Project Description

ChronoTrack is an **administrative audit tool** designed for instructors to log their professional sessions with high precision. Its core purpose is not general-purpose scheduling or planning — it is a **record-of-fact system**: every entry represents a completed session that must be captured accurately, preserved without alteration, and made available for downstream administrative processes such as payroll and compliance reporting.

The system's design philosophy is built around three pillars:

- **Accuracy** — session data (date, start time, end time, duration) must be captured precisely at the point of entry.
- **Immutability** — once logged, session history must be protected from accidental or unauthorized modification, preserving its value as an audit trail.
- **Exportability** — session data must be retrievable in a structured, standardized format suitable for administrative and payroll systems.

ChronoTrack's UI has already been designed; this document defines the underlying project scope, user stories, and functional/business logic that the development team will implement against those designs.

---

## 2. User Personas

### 2.1 The Instructor

| Attribute | Description |
|---|---|
| **Role** | Primary and sole end user of ChronoTrack |
| **Technical Proficiency** | Low to moderate; expects a simple, low-friction interface |
| **Primary Need** | Quickly and accurately log session time without ambiguity |
| **Secondary Need** | Trust that once logged, their history is a reliable, tamper-proof record |
| **Reporting Need** | Ability to retrieve their own data in a portable format (Excel) for submission to administrative bodies |

**Key Persona Insight:** The Instructor is not a data analyst or administrator — they are a subject-matter expert whose interaction with ChronoTrack is transactional (log a session) and occasional (review/export history). The system must therefore minimize data-entry friction while maximizing the trustworthiness of what gets recorded. Because payroll and compliance outcomes depend on this data, the Instructor also has a strong interest in the system preventing accidental edits — an unintentional change to past history could call the Instructor's own records into question.

---

## 3. User Stories

| ID | User Story | Acceptance Criteria (Summary) |
|---|---|---|
| US-01 | As an Instructor, I want to log a session with a specific date and time range so my work is accurately recorded. | User can select a date, a start time, and an end time; system validates the range and calculates duration automatically upon save. |
| US-02 | As an Instructor, I want to view a read-only history of my sessions so I can audit my past work without accidental edits. | Sessions History view renders all past entries in a list/table; no edit or delete controls are exposed to the Instructor role. |
| US-03 | As an Instructor, I want to filter my history by specific dates or months so I can generate reports for specific periods. | User can apply a date-range filter or select a specific month; the displayed list updates to reflect only matching sessions. |
| US-04 | As an Instructor, I want to export my filtered data to an Excel sheet so I can submit it for payroll/compliance. | Export action generates an `.xlsx` file reflecting the currently applied filter, with the standardized column structure defined in Section 4.3. |

---

## 4. Functional Requirements & Logic

### 4.1 Authentication

**4.1.1 Login Flow**

1. Instructor enters registered email/username and password.
2. System validates credentials against stored (hashed) credentials.
3. On success: Instructor is routed to the Sessions Dashboard.
4. On failure: system returns a generic error ("Invalid email or password") — the system must **not** reveal whether the email or the password was incorrect, to prevent user enumeration.
5. Failed login attempts should be rate-limited (e.g., lockout or exponential backoff after N consecutive failures) to mitigate brute-force attempts.

**4.1.2 OTP-Based Password Reset Flow**

1. **Request Reset:** Instructor selects "Forgot Password" and submits their registered email address.
2. **OTP Generation:** System generates a time-bound One-Time Password (numeric, e.g., 6 digits) and sends it to the registered email.
   - OTP validity window: recommended 5–10 minutes.
   - OTP must be single-use; once consumed (or expired), it is invalidated.
3. **OTP Verification:** Instructor enters the OTP on the reset screen.
   - System validates the OTP against the stored value and expiry timestamp.
   - A maximum retry count should be enforced (e.g., 3–5 attempts) before requiring a new OTP request.
4. **Password Reset:** Upon successful OTP verification, Instructor is prompted to set a new password (subject to password policy: minimum length, complexity, etc.).
5. **Confirmation:** System confirms the password has been updated and invalidates any existing sessions, requiring re-login with the new credentials.

> **Security Note:** The system must never expose whether an email address exists in the database during the reset request step — the confirmation message should be generic (e.g., "If this email is registered, an OTP has been sent.").

---

### 4.2 Time Calculation Logic

**4.2.1 Duration Calculation**

```
Duration = End Time − Start Time
```

- Both `Start Time` and `End Time` are captured at the granularity of minutes (HH:MM).
- Duration is expressed in decimal hours or HH:MM format (to be confirmed with UI team based on existing designs), computed at the moment of session save.
- **Validation Rule:** `End Time` must be strictly greater than `Start Time` within the same logged date. Overnight sessions (crossing midnight) should either be disallowed at entry or explicitly supported via a "next day" flag — this must be clarified with stakeholders before implementation, as it materially affects the calculation logic.

**4.2.2 Time Rounding Logic**

Time Rounding is a derived field, separate from the raw Duration, used to standardize reported hours for payroll purposes. Both the raw **Duration** and the derived **Time Rounding** value are persisted and displayed together — the raw value supports audit integrity, while the rounded value supports payroll reporting.

**Confirmed Business Rule:**

Time Rounding is calculated from the raw Duration by extracting the completed **hour** component (`H`) and the remaining **minutes** component (`M`), then applying a 20-minute threshold:

```
IF H = 3:
    Time Rounding = 3 hours

ELSE IF H <= 2:
    IF M >= 20:
        Time Rounding = 3 hours
    ELSE:
        Time Rounding = 2 hours
```

**Interpretation:**

- A session whose duration reaches a full **3 hours** (`H = 3`) is rounded to **3 hours** — no further check is needed.
- For sessions where the hour component is **2 or fewer** (`H <= 2`), the system inspects the leftover minutes:
  - If the leftover minutes are **20 or more**, the session rounds **up to 3 hours**.
  - If the leftover minutes are **under 20**, the session rounds **down to 2 hours** only.

**Worked Examples:**

| Raw Duration | H (hours) | M (minutes) | Time Rounding |
|---|---|---|---|
| 3h 00m | 3 | 0 | 3 hours |
| 2h 45m | 2 | 45 | 3 hours (M ≥ 20) |
| 2h 20m | 2 | 20 | 3 hours (M ≥ 20) |
| 2h 15m | 2 | 15 | 2 hours (M < 20) |
| 1h 50m | 1 | 50 | 2 hours (M ≥ 20) |
| 0h 10m | 0 | 10 | 2 hours (M < 20) |

> **Note for Dev Team:** As specified, this rule is defined around the 2–3 hour session band (the organization's standard session length). If sessions of other lengths (e.g., 4+ hours, or sub-1-hour sessions) are expected to occur in practice, confirm with stakeholders whether the same `H`/`M` extraction and 20-minute threshold pattern should extend to those bands, or whether a different rule applies outside this range.

---

### 4.3 Export Logic

- Export is available from the Sessions History view and respects any active filter (date range or month) applied by the Instructor.
- Export format: **Excel (.xlsx)**.
- The exported file **must** preserve the following column structure, in this exact order, regardless of filter applied:

| Session # | Date | From | To | Duration | Time Rounding |
|---|---|---|---|---|---|
| Sequential identifier for the session (per Instructor, or globally — TBD) | Session date (format to match existing UI convention) | Start time | End time | Raw calculated duration | Rounded duration per 4.2.2 |

- **Session #** should be a stable, sequential reference — this allows administrative staff to cross-reference specific rows if a discrepancy needs to be investigated, and should not change if unrelated filters are applied to the same underlying dataset.
- Column headers must remain fixed and unchanged across exports to guarantee compatibility with downstream administrative/payroll ingestion processes.
- If the filtered result set is empty, the export should still generate a valid file containing headers only (not an error), so downstream automation does not break on empty periods.

---

### 4.4 Data Integrity — Read-Only Session History

- The **Sessions History** view is strictly **read-only** for the Instructor role. No UI affordances (edit icons, delete buttons, inline editing) should be exposed to Instructors on this screen or any session row within it.
- This constraint exists specifically to preserve **audit compliance** — once a session is logged, it becomes part of the permanent record used for administrative and payroll reporting, and must not be alterable by the party being audited.
- Any correction to a previously logged session (e.g., due to a genuine data-entry error) must be handled through a separate, administrator-mediated process outside the scope of the Instructor-facing application (e.g., an admin panel or support request workflow) — this ensures a clear separation of duties between the person logging time and the person authorized to amend it.
- At the data layer, this should be enforced not only via UI restriction but via **backend authorization rules** (i.e., the API must reject update/delete requests against session records originating from an Instructor-role token), since UI-only restrictions can be bypassed.

---

## 5. Summary of Open Items Requiring Stakeholder Confirmation

1. Whether the confirmed Time Rounding rule (Section 4.2.2) should extend beyond the 2–3 hour band to other session lengths.
2. Handling of overnight sessions that cross midnight (Section 4.2.1).
3. Scope of "Session #" sequencing — per-instructor or global (Section 4.3).
4. Display format for Duration (decimal hours vs. HH:MM) — to align with existing UI designs.

---
