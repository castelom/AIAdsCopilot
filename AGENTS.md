# AI Ads Copilot — Codex Agent Instructions

## Mission
Build a lean, testable **CSV-first Google Ads audit engine** for small businesses. The MVP analyzes Google Search campaign exports and a commercial lead spreadsheet, generates deterministic findings, and later uses an LLM **only to explain grounded findings**. Campaign changes remain the agency's responsibility.

## Communication and token discipline
- Respond in **Brazilian Portuguese**; keep code identifiers and technical documentation in English.
- Be concise. Start with the decision/result; avoid introductions, repeated summaries, and speculative alternatives.
- Inspect only files relevant to the task. Prefer targeted searches (`rg`, explicit paths) over reading the entire repository.
- Before coding, state a plan of at most **3 short steps** only for nontrivial changes. For simple changes, implement directly.
- Make the smallest coherent change. Reuse existing patterns; do not refactor unrelated code.
- Do not repeat large file contents or dump logs. Show paths, brief change summaries, and essential errors only.
- Ask questions only when an ambiguity blocks a correct implementation; otherwise state a reasonable assumption.
- Do not add dependencies, services, abstractions, or new features without a concrete need.
- When blocked, state the blocker and the minimum action needed. Never claim tests passed if not executed.
- Final response format: **Changed / Verified / Pending**; omit empty sections. Keep each section short.

## MVP scope
**In scope**
1. Import Google Ads CSV exports (campaign, ad group, search terms, and conversion breakdown as available).
2. Import commercial leads CSV from the existing Google Sheet.
3. Validate schemas, dates, decimal/currency formats, and missing data; normalize into typed domain models.
4. Calculate metrics (spend, impressions, clicks, CTR, CPC, reported conversions, reported CPA) with explicit denominators.
5. Run configurable deterministic audit rules and produce evidence-backed findings.
6. Export a machine-readable `analysis-result.json` through a CLI.
7. Add LLM interpretation only **after** the deterministic engine is verified.

**Out of scope for v1**: frontend, Google Ads API, WhatsApp API, automatic campaign edits, multi-tenant SaaS, dashboards, scheduled jobs, autonomous agents, RAG, vector databases, microservices, and speculative infrastructure.

## Proposed solution layout
```text
AIAdsCopilot/
├── src/
│   ├── AIAdsCopilot.Cli/
│   ├── AIAdsCopilot.Application/
│   ├── AIAdsCopilot.Domain/
│   └── AIAdsCopilot.Infrastructure/
├── tests/
│   └── AIAdsCopilot.Tests/
├── samples/
│   ├── google-ads/
│   └── leads/
└── README.md
```
Use **.NET / C#**, with domain rules independent of CSV parsing and CLI concerns. Prefer built-in .NET facilities; CsvHelper is acceptable when needed. Start with a simple local execution path; do not introduce a database unless a demonstrated requirement demands it.

## Domain truths and attribution rules
- **An ad click is not a lead. A site visit is not a lead. A WhatsApp button click is not proof that a message was sent.**
- `Conversation started` from click-to-message ads indicates an initiated conversation in that ad flow, **not** confirmation of a qualified lead or a site WhatsApp message.
- Calls are contact signals, not automatically qualified leads.
- `Conversions` may aggregate actions of different commercial value. Do not equate platform-reported CPA with cost per qualified lead.
- The business's website WhatsApp button has **no prefilled message**. The distinct prefilled message is used for contacts from the Google ad flow; the attendant labels those `Google Ads`. For messages without that identifier, the attendant asks the customer for their source.
- The current lead spreadsheet has an `Origem` field but **does not reliably preserve the attribution method** for every row. Do not infer `TRACKED_ADS` or high attribution confidence from the label `Google Ads` alone.
- A site contact may have originated from paid or organic traffic; `SITE` does not mean organic.
- Do not link an individual lead to a specific campaign without a reliable identifier. Do not infer duplicate contacts or conversion events without evidence.
- Missing lead status means **unknown**, not unqualified. No confirmed qualified leads in a partial sample does not mean zero qualified leads.
- Keep personal information out of logs, sample fixtures, LLM prompts, and reports unless strictly necessary. Prefer anonymized input samples.

## Historical reference case — NOT hard-coded business logic
Google Search campaign `M - [PESQUISA][NOVO SITE] - 05/11`, observed activity Nov 5–6, 2026:
- Spend: approximately **BRL 358.70**; clicks: **84**; impressions: **836**.
- Reported primary conversions: **19**; reported CPA: approximately **BRL 18.88**.
- Breakdown: **13** site WhatsApp button clicks (`M - BOTÃO WPP (1)`), **5** click-to-message `Conversation started`, **1** `Calls from ads`.
- Bid strategy: **target CPA BRL 8.12**; all examined conversion actions are configured as primary. Treat the short observation period as insufficient for strong performance conclusions.
- The lead sheet uses fields such as `Data`, `Hora`, `Nome completo`, `Telefone / WhatsApp`, `Origem`, `Tipo de serviço`, `Status`, `Observação`, `Valor fechado`.
- The reported CPA is based on **19 mixed conversion events**, not 19 verified qualified leads.
Use these numbers only as **test fixtures/reference assertions**; all production computations must come from input data.

## Deterministic audit rules (initial candidates)
- `R01`: Clearly irrelevant search terms relative to the advertiser's service catalog; ambiguous terms require review, not automatic exclusion.
- `R02`: Meaningful spend without attributed conversions; require minimum data/sample thresholds.
- `R03`: Reported CPA above configured target; qualify conclusions based on observation period and volume.
- `R04`: High proportion of commercially unqualified leads; do not conflate missing statuses with disqualification.
- `R05`: Conversion measurement inconsistencies or potentially overlapping goals; report as **audit hypotheses**, not proven duplicates.
- `R06`: Insufficient data; suppress overconfident conclusions.
- `R07`: Primary conversion goals measure low-intent actions, such as contact-button clicks.
- `R08`: Contact-button conversion without confirmation of message submission.
Every finding must include `ruleId`, `severity`, `summary`, `evidence`, `limitations`, and `recommendedAction`. Severity must reflect evidence strength, not just numeric deviation. Thresholds belong in configuration, never scattered magic numbers.

## Engineering rules
- Use invariant internal types and explicit Brazilian CSV parsing (decimal comma, `R$`, delimiters, date formats, encoding). Do not silently coerce malformed input.
- Treat different Google Ads export scopes and date windows explicitly. Never combine mismatched periods without flagging it.
- Avoid divide-by-zero and label undefined metrics as unavailable rather than zero.
- Separate **measured facts**, **derived calculations**, **hypotheses**, and **recommendations**.
- Preserve traceability: each finding should identify the input file/row or aggregate that supports it, without exposing customer PII.
- Tests must cover metric formulas, conversion-type separation, attribution uncertainty, missing status, insufficient sample, malformed CSV, and period mismatch.
- Use unit tests for domain rules and small integration tests for CSV-to-JSON processing.
- No external API calls, destructive operations, or changes to campaign settings unless explicitly requested and authorized.

## LLM phase (later)
- Input: compact structured JSON of validated metrics and findings, **not** raw CSVs or personal data.
- Output: concise Portuguese explanations and prioritized agency recommendations.
- Never allow the LLM to invent metrics, overwrite deterministic findings, claim causation, or silently promote hypotheses to facts.
- Validate structured LLM output against a schema; keep a deterministic report available when the LLM is unavailable.

## Working agreement
For each requested change:
1. Check existing implementation and constraints.
2. Implement the minimum testable increment.
3. Run the most relevant tests; report actual results and any remaining uncertainty.

If a request conflicts with this file, follow the user's explicit current instruction and flag the scope change briefly.
