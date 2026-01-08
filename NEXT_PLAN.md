# Plan de Développement Frontend - Niko Niko Calendar

Ce document décrit le plan pour les prochaines modifications des écrans de l'application frontend, en respectant les principes de Material Design et l'utilisation de la bibliothèque Material UI.

## Objectif

Modifier les écrans de l'application frontend pour améliorer l'expérience utilisateur et l'esthétique, conformément aux maquettes fournies par l'utilisateur.

## Méthodologie

1.  **Fourniture des Mockups par l'Utilisateur** :
    *   L'utilisateur hébergera les images des maquettes en ligne (par exemple, sur Imgur, Postimages, ou un Gist GitHub).
    *   L'utilisateur fournira les URLs de ces images à l'agent.
    *   **Note importante** : L'agent ne peut pas interpréter visuellement des fichiers image locaux. La fourniture d'URLs est essentielle pour l'analyse.

2.  **Analyse des Mockups par l'Agent** :
    *   L'agent examinera les maquettes fournies via leurs URLs.
    *   Cette analyse permettra de comprendre les changements de design, l'agencement, les composants d'interface utilisateur spécifiques à utiliser et le style général attendu.

3.  **Proposition d'un Plan d'Implémentation par l'Agent** :
    *   L'agent rédigera un plan détaillé des modifications à effectuer.
    *   Ce plan inclura :
        *   Les composants React (`.tsx`) qui devront être créés, modifiés ou supprimés.
        *   Les composants Material UI (MUI) spécifiques (par exemple, `<Card>`, `<Grid>`, `<Button>`, `<TextField>`, etc.) à utiliser pour chaque élément de la maquette.
        *   La logique et les ajustements de style à implémenter.
        *   Les fichiers concernés dans le répertoire `app/frontend/src/`.

4.  **Validation du Plan par l'Utilisateur** :
    *   L'utilisateur examinera le plan proposé par l'agent.
    *   L'agent attendra la validation de l'utilisateur avant de procéder à toute modification de code.

5.  **Développement par l'Agent** :
    *   Une fois le plan validé, l'agent procédera à l'implémentation des changements dans le code du frontend.
    *   Les modifications respecteront strictement les conventions de code existantes et les principes de Material Design, en utilisant Material UI.

6.  **Vérification par l'Agent (et l'Utilisateur)** :
    *   L'agent s'assurera de la qualité du code en exécutant les outils de linting (`npm run lint`).
    *   L'utilisateur sera invité à vérifier visuellement les changements en lançant l'application frontend localement pour confirmer la conformité avec les maquettes et l'objectif visé.

---
**Prochaine Étape :** Fournir les URLs des maquettes pour que l'agent puisse commencer l'analyse.


---

## Proposition Ascii-Mocku

 ┌──────────────────────────────────────────────────────────────────────────────────────────┐  
 │┌──────────────────┐                                                                      │  
 ││                  │  Team 1 - Current sprint                                             │  
 ││ Dashboard        │  sprint date start / end                                             │  
 ││                  │                                                                      │  
 ││   > Current      │ ┌──────────────────────────────────────────────────────────────────┐ │  
 ││                  │ │ team user 1  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││   Past sprints   │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││                  │ │                                                                  │ │  
 ││   Invitation     │ │ team user 2  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││                  │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││                  │ │                                                                  │ │  
 ││                  │ │ team user 3  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││                  │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││                  │ └──────────────────────────────────────────────────────────────────┘ │  
 ││ Adminstration    │                                                                      │  
 ││                  │  Team 2 - Current sprint                                             │  
 ││   Teams          │  sprint date start / end                                             │  
 ││                  │                                                                      │  
 ││   Users          │ ┌──────────────────────────────────────────────────────────────────┐ │  
 ││                  │ │ team user 1  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││   Sprints        │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││                  │ │                                                                  │ │  
 ││                  │ │ team user 6  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││                  │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││┌───┐             │ │                                                                  │ │  
 │││   │ UserName    │ │ team user 8  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││└───┘             │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 │└──────────────────┘ └──────────────────────────────────────────────────────────────────┘ │  
 └──────────────────────────────────────────────────────────────────────────────────────────┘  
                                                                                               
 ┌──────────────────────────────────────────────────────────────────────────────────────────┐  
 │┌──────────────────┐                                                                      │  
 ││                  │  Team 1 - Previous sprint -1                                         │  
 ││ Dashboard        │  sprint date start / end                                             │  
 ││                  │                                                                      │  
 ││   Current        │ ┌──────────────────────────────────────────────────────────────────┐ │  
 ││                  │ │ team user 1  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││   > Past sprints │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││                  │ │                                                                  │ │  
 ││   Invitation     │ │ team user 2  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││                  │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││                  │ │                                                                  │ │  
 ││                  │ │ team user 3  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││                  │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││                  │ └──────────────────────────────────────────────────────────────────┘ │  
 ││ Adminstration    │                                                                      │  
 ││                  │  Team 1 - Previous sprint -2                                         │  
 ││   Teams          │  sprint date start / end                                             │  
 ││                  │                                                                      │  
 ││   Users          │ ┌──────────────────────────────────────────────────────────────────┐ │  
 ││                  │ │ team user 1  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││   Sprints        │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││                  │ │                                                                  │ │  
 ││                  │ │ team user 6  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││                  │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 ││┌───┐             │ │                                                                  │ │  
 │││   │ UserName    │ │ team user 8  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐ │ │  
 ││└───┘             │ │              └─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘└─┘ │ │  
 │└──────────────────┘ └──────────────────────────────────────────────────────────────────┘ │  
 └──────────────────────────────────────────────────────────────────────────────────────────┘  
                                                                                               