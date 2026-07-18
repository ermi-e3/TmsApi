# API Versioning Policy

## Purpose

This document defines how the TMS API is versioned and how changes are introduced without unnecessarily breaking existing client applications. The goal is to provide predictable upgrades while maintaining backward compatibility whenever possible.

---

# Breaking Changes

A new API version **must** be released when a change can cause an existing client to stop working without any modification.

Examples of breaking changes include:

- Removing an existing endpoint.
- Removing a response field.
- Renaming a request or response field.
- Changing the data type of a field.
- Changing an endpoint's URL or route.
- Changing HTTP status codes for existing scenarios.
- Tightening validation rules that reject requests previously accepted.
- Changing authentication or authorization requirements.
- Changing the default sort order or pagination behavior in a way that alters existing client expectations.
- Making an optional request parameter required.

---

# Non-Breaking (Additive) Changes

The following changes are considered backward compatible and do not require a new API version:

- Adding a new optional response field.
- Adding a new endpoint.
- Adding a new optional query parameter.
- Adding new optional request properties.
- Improving performance.
- Fixing bugs without changing the API contract.
- Adding new documentation or examples.

Existing clients should continue working without modification.

---

# Version Lifetime (Sunset Policy)

Each major API version will remain supported for **at least six months** after its successor is released.

For example:

- Version 2 is released.
- Version 1 continues to operate for a minimum of six months.
- This allows training centres and partner organizations sufficient time to upgrade their applications according to their maintenance schedules.

After the sunset period, the older version may be permanently removed.

---

# Deprecation Communication

When a newer version is released, the TMS team will notify API consumers through multiple channels.

Communication includes:

- `Deprecation` response header.
- `Sunset` response header.
- `Link` header pointing to migration documentation.
- CHANGELOG entry describing the changes.
- Email notification to every registered API consumer.
- Calendar invitation announcing the planned shutdown date of the deprecated version.

These notifications begin on the day the new version becomes available.

---

# Version Migration

Clients are not required to upgrade through every intermediate version.

For example:

- V1 → V3 is supported.
- V2 is optional.
- Clients may migrate directly to the latest supported version whenever they choose.

Migration guides will be provided for every major version.

---

# Versioning Principles

The TMS API follows these principles:

- Avoid breaking changes whenever possible.
- Prefer additive changes over modifying existing behavior.
- Release a new major version only when required.
- Maintain compatibility for the published support period.
- Provide clear documentation before removing any API version.

# Header-Based Versioning

The primary versioning strategy for the TMS API is URL segment versioning.

Examples:

GET /api/v1/courses
GET /api/v2/courses

This approach makes the API version immediately visible in logs, monitoring tools, browser history, and incident reports.

The API also supports header-based versioning using the `X-Api-Version` request header as an optional compatibility mechanism.

Example:

GET /api/courses
X-Api-Version: 2.0

Header-based versioning is available only for approved integration partners whose environments cannot easily support URL versioning (for example, mobile applications or systems with cached CDN routes).

It is an opt-in feature and is not the default versioning strategy. All new clients should use URL segment versioning unless an exception has been approved.