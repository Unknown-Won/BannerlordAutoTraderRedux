# Translating AutoTrader Redux

Every text the mod shows in game (menu options, the settings screen, messages) comes from
`AutoTrader/ModuleData/Languages/module_strings.xml`. That English file is the template for a
translation. Only debug log lines stay in English.

## Make a translation

1. In the installed mod, open `Modules/AutoTrader/ModuleData/Languages` (or the same folder in your
   mod manager's copy of AutoTrader).
2. Make a folder for your language next to `module_strings.xml`, for example `TR` for Turkish.
3. Copy `module_strings.xml` into that folder and rename the copy, for example `module_strings_tr.xml`.
4. In the copy, change `<tag language="English" />` to your language's name as the game spells it
   (see the table below), then translate each `text="..."` value.
5. In the same folder, create `language_data.xml`:

   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <LanguageData id="Türkçe">
     <LanguageFile xml_path="TR/module_strings_tr.xml" />
   </LanguageData>
   ```

   `id` must be exactly the game's name for the language. `xml_path` is relative to the `Languages`
   folder, so it starts with your folder name.
6. Save both files as UTF-8, start the game with that language selected, and check the AutoTrader
   menu options and settings screen (Alt + A on the campaign map).

The result looks like this:

```
AutoTrader/ModuleData/Languages/
  language_data.xml          (English, part of the mod)
  module_strings.xml         (English, part of the mod)
  TR/
    language_data.xml        (your file)
    module_strings_tr.xml    (your translation)
```

## Rules for the texts

- Keep every `id` unchanged. Only `text` is translated.
- Keep words in braces as they are, for example `{NAME}` or `{HOTKEY}`: the mod puts the profile
  name, hotkey or version there. You can move them within the sentence.
- Inside `text="..."`, write `&quot;` for `"`, `&amp;` for `&`, `&lt;` for `<` and `&gt;` for `>`.
- An id you leave out shows in English, so a partial translation still works.
- After a mod update, compare your file with the new English `module_strings.xml` and add any new
  ids. Ids whose English text changed in meaning get a new id, so old translations never show a
  wrong text.

## Language names

Use the `id` from the game's own file for your language, found in
`Modules/Native/ModuleData/Languages/<folder>/language_data.xml` in the game folder. The usual
folders are `BR`, `CNs`, `CNt`, `DE`, `FR`, `IT`, `JP`, `KO`, `PL`, `RU`, `SP` and `TR`
(Turkish: `Türkçe`).

## Sharing a translation

Upload it as its own mod that contains only `AutoTrader/ModuleData/Languages/<folder>/` and
requires AutoTrader Redux, or send it to the AutoTrader Redux author to include in the mod.
