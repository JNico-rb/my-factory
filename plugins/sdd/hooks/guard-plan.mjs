// PreToolUse hook (Edit|Write|MultiEdit): tests and code are written only for a step of an approved
// plan (AGENTS.md, gate "Write tests or code"). Code is what lives under the `codeDirs` listed in
// .claude/sdd.json; without that file, or with the list empty, the project has no guarded code yet and
// every call passes. A write under a code dir passes only when some specs/*/plan.md has its approval
// box marked and a step still open. Exit 2 blocks the call; stderr is the reason handed to the model.
import { existsSync, readdirSync, readFileSync } from "node:fs";

// Windows paths arrive with backslashes and either case of drive letter; compare them as a/b/c.
const normalize = (p) => p.replace(/\\/g, "/").replace(/^([a-z]):/, (_, d) => `${d.toUpperCase()}:`).replace(/\/$/, "");
const isAbsolute = (p) => p.startsWith("/") || /^[A-Z]:\//.test(p);
const samePrefix = (file, dir) =>
  process.platform === "win32" ? file.toLowerCase().startsWith(`${dir.toLowerCase()}/`) : file.startsWith(`${dir}/`);

const input = JSON.parse(readFileSync(0, "utf8"));
const target = input.tool_input?.file_path;
if (!target) process.exit(0);

const root = normalize(process.env.CLAUDE_PROJECT_DIR ?? input.cwd ?? process.cwd());
const configPath = `${root}/.claude/sdd.json`;
if (!existsSync(configPath)) process.exit(0);
const codeDirs = (JSON.parse(readFileSync(configPath, "utf8")).codeDirs ?? []).map((d) => normalize(d).replace(/^\.\//, ""));

const cwd = normalize(input.cwd ?? root);
const file = isAbsolute(normalize(target)) ? normalize(target) : `${cwd}/${normalize(target)}`;
if (!samePrefix(file, root)) process.exit(0);
const rel = file.slice(root.length + 1);

// A code dir's own AGENTS.md or CLAUDE.md is an instruction file, not code.
const guarded = codeDirs.some((d) => samePrefix(rel, d)) && !/(^|\/)(AGENTS|CLAUDE)\.md$/.test(rel);
if (!guarded) process.exit(0);

const plans = existsSync(`${root}/specs`)
  ? readdirSync(`${root}/specs`, { withFileTypes: true })
      .filter((e) => e.isDirectory() && existsSync(`${root}/specs/${e.name}/plan.md`))
      .map((e) => [e.name, readFileSync(`${root}/specs/${e.name}/plan.md`, "utf8")])
  : [];
const isOpen = (plan) => {
  if (!/^- \[[xX]\] Plan approved/m.test(plan)) return false;
  const steps = plan.split(/^### Steps\s*$/m)[1]?.split(/^### /m)[0] ?? "";
  return /^- \[ \] /m.test(steps);
};
if (plans.some(([, plan]) => isOpen(plan))) process.exit(0);

const approvedDone = plans.filter(([, plan]) => /^- \[[xX]\] Plan approved/m.test(plan)).map(([name]) => name);
const why = approvedDone.length
  ? `every approved plan (${approvedDone.join(", ")}) has all its steps [x], so no plan asks for this code`
  : "no specs/*/plan.md has its box \"- [x] Plan approved\" marked";
process.stderr.write(
  `guard-plan: blocked ${rel}: ${why}. Tests and code are written only for an open step of an approved plan ` +
    "(AGENTS.md, Approval gates). Next: write the spec and plan (workflow/2-specs.md, workflow/3-plan.md) " +
    "and ask the user to mark the plan's approval box; only the user marks it.\n",
);
process.exit(2);
