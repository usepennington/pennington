---
title: "Formal grammar"
description: "EBNF-style formal grammar excerpt for Bramble programs, function declarations, statements, expressions, and patterns."
uid: bramble.reference.language.grammar
order: 150
sectionLabel: "Language"
tags: [grammar, EBNF, syntax, formal, parsing]
---

This page provides an EBNF-style grammar excerpt for Bramble 1.2. It covers the top-level program structure, function declarations, statements, expressions, and patterns. The grammar is intended as a precise reference; it is not a tutorial introduction to the syntax.

## Notation

- `A B` — sequence
- `A | B` — alternation
- `A?` — zero or one
- `A*` — zero or more
- `A+` — one or more
- `( A )` — grouping
- `"x"` — literal terminal
- `UPPER` — lexical terminal defined separately

## Grammar excerpt

```text
program       ::= (import_decl | item_decl)*

import_decl   ::= "import" module_path ("as" IDENT)?

item_decl     ::= fn_decl | struct_decl | enum_decl | trait_decl

fn_decl       ::= "pub"? "fn" IDENT
                  "(" (param ("," param)* ","?)? ")"
                  ("->" type)?
                  block

param         ::= "mut"? IDENT ":" type

struct_decl   ::= "pub"? "struct" IDENT "{" (field ("," field)* ","?)? "}"

field         ::= IDENT ":" type

enum_decl     ::= "pub"? "enum" IDENT
                  "{" (variant ("," variant)* ","?)? "}"

variant       ::= IDENT ( "{" (field ("," field)* ","?)? "}" )?

statement     ::= let_stmt
                | assign_stmt
                | expr_stmt
                | return_stmt

let_stmt      ::= "let" "mut"? IDENT (":" type)? "=" expr

assign_stmt   ::= place assign_op expr

assign_op     ::= "=" | "+=" | "-=" | "*=" | "/=" | "%="
                | "&=" | "|=" | "^="

return_stmt   ::= "return" expr?

expr_stmt     ::= expr

expr          ::= literal
                | IDENT
                | expr "." IDENT                   // field access
                | expr "." IDENT "(" args ")"      // method call
                | IDENT "(" args ")"               // function call
                | expr binary_op expr
                | unary_op expr
                | expr "as" type                   // cast
                | expr "?"                         // propagation
                | block
                | if_expr
                | while_expr
                | for_expr
                | match_expr
                | "(" expr ("," expr)* ")"         // tuple
                | "[" (expr ("," expr)* ","?)? "]" // list
                | "{" (map_entry ("," map_entry)* ","?)? "}" // map
                | IDENT "{" (field_init ("," field_init)* ","?)? "}"  // struct

if_expr       ::= "if" expr block ("else" (if_expr | block))?

while_expr    ::= "while" expr block

for_expr      ::= "for" "mut"? IDENT "in" expr block

match_expr    ::= "match" expr "{" (arm ("," arm)* ","?)? "}"

arm           ::= pattern "=>" expr

block         ::= "{" statement* expr? "}"

pattern       ::= "_"
                | "true" | "false"
                | INTEGER_LIT | STRING_LIT | CHAR_LIT
                | IDENT                            // binding
                | IDENT "{" (field_pat ("," field_pat)* ","?)? "}"  // struct
                | IDENT "(" (pattern ("," pattern)* ","?)? ")"      // enum variant
                | "Some" "(" pattern ")"
                | "None"
                | "Ok" "(" pattern ")"
                | "Err" "(" pattern ")"
                | pattern "|" pattern              // or-pattern
                | pattern "if" expr               // guard

type          ::= "i64" | "f64" | "bool" | "char" | "str" | "()"
                | "[" type "]"
                | "{" type ":" type "}"
                | "{" type "}"
                | "(" type ("," type)+ ")"
                | "Option" "<" type ">"
                | "Result" "<" type "," type ">"
                | "fn" "(" (type ("," type)*)? ")" ("->" type)?
                | IDENT ( "<" type ("," type)* ">" )?
```

## Notes

**Trailing commas** are allowed in all comma-separated lists (parameter lists, argument lists, struct fields, match arms). The formatter (`sprig`) normalises multi-line lists to always include a trailing comma.

**Block as expression** — every `block` production may appear wherever an `expr` is expected. The value of the block is its final `expr`, or `()` if the block ends with a statement.

**Pattern guards** — the `if` guard in a match arm is evaluated only when the pattern itself matches. Guards do not affect exhaustiveness checking; the compiler still requires the set of patterns to be exhaustive independent of any guards.

**Operator grammar** — binary and unary operators are not expanded inline above for brevity. Their precedence and associativity are specified in [operators](xref:bramble.reference.language.operators).
