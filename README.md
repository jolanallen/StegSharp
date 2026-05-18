# StegSharp

StegSharp est une application de bureau multiplateforme (Windows, Linux) développée en C# avec le framework .NET 10. Elle permet de dissimuler des messages textuels secrets au sein d'images numériques en exploitant la méthode de stéganographie LSB (Least Significant Bit). 

Ce dépôt contient le code source de l'application, réalisé dans le cadre d'un projet étudiant.

## Fonctionnalités principales

- Encodage stéganographique : Insertion d'un texte dans les pixels d'une image sans altération visuelle perceptible.
- Décodage : Extraction d'un message préalablement dissimulé dans une image.
- Validation des capacités : Calcul en temps réel de la taille maximale du message qu'une image peut contenir.
- Architecture modulaire : Séparation stricte des responsabilités entre la logique métier et l'interface utilisateur.
- Journalisation (Logging) : Suivi des événements et gestion des erreurs via Serilog (fichiers tournants et console).

## Architecture du projet

La solution (`SteganoApp.slnx`) est divisée en deux sous-projets distincts afin de respecter les principes de la "Clean Architecture" :

1. Stegano.Core
Bibliothèque de classes contenant le cœur de l'application. Elle est indépendante de toute technologie d'affichage.
- `Services/` : Logique d'encodage, de décodage et manipulation LSB (`LsbSteganographyService`).
- `Repositories/` : Abstraction de l'accès aux fichiers (chargement et sauvegarde des images en mémoire).
- `DTOs/` : Objets de transfert de données assurant la communication avec l'interface.

2. Stegano.UI
Projet exécutable gérant l'interface graphique.
- Développé avec le framework Avalonia UI pour garantir une compatibilité native sous Windows et Linux.
- Implémente le pattern MVVM (Model-View-ViewModel) / Code-Behind pour l'interaction avec le `Stegano.Core`.

## Prérequis et Installation

### Environnement de développement
Pour compiler et exécuter le projet depuis les sources, vous devez installer :
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Compilation

Clonez le dépôt et naviguez dans le dossier racine :
```bash
git clone https://github.com/jolanallen/StegSharp.git
cd StegSharp
```
