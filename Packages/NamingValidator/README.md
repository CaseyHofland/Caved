## Possible Improvements
**MAKE THE ANALYZER RUN ASYNCHRONOUS.**

Presumably this could be done simply by calling `Task.Run(NamingAnalyzer.AnalyzeProject)` inside the NamingProcessor, adding some [Progress Reports](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/Progress.Report.html) as it goes along, but this would need researching as I have little experience with asynchronous code.

---

**FIND NON-MATCHING CHARACTERS TO PROVIDE BETTER LOG MESSAGES.**

Right now the error message just tells you that a string is wrong, but it doesn't tell you what points of the string aren't excepted. Adding this would provide more transparency as to how the string needs to change in order to pass validation. [Use this as a starting point.](https://stackoverflow.com/questions/12383945/find-not-matching-characters-in-a-string-with-regex)

---

**PROVIDE ONE ERROR LOG THAT LINKS TO A FILE WITH ALL NAMING ERRORS.**

When someone has 30 naming errors, it cloggs the whole console every time you save, making it impossible to find out which errors are actually important. It would be better to log 1 error to the console instead, and create a file inside the logs folder in which people can find all naming errors and provided data (bonus: perhaps this could be a .csv file with tables for clarity).

---

**USE PRESETS CHECKS INSTEAD OF FLAWED TYPE CHECKS**

Currently the validator validates by type, but this has a couple of issues. Some types overlap (e.g. prefabs and models both derive from type "GameObject") and you can't specify naming patterns for assets with certain values (as is the case with Normal Maps, that are of type "Texture" but with their Texture Type set to "Normal map").

Incidentally, this would enclose the gap between adding presets for assets with certain names and applying those presets on import when assets are named correctly (which is existing Unity functionality).

---

**QOL IMPROVEMENTS**

**Found in Edit > Project Settings > Naming Analyzer**
- Directory Wildcards (e.g. **/Prototype).
- Drag & Drop on the Exclude from Naming Analyzers list.

**Found in Asset Menu > Create > Naming Ruleset**
- Create a button next to "pattern" that links you to a regex tester web page with your pattern as input.

**Found in Context Menu**
- "Obey Naming Validator" and "Ignore Naming Validator" are semantically correct, but confusing. Rename these to better things.
