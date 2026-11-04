# Tomato Leaf Disease Classification — README

SCOOP project by Davide Pomaro (2191101). This file explains how to run the
program; for the description of the project see the report
(SCOOP_Project_Report_Pomaro.pdf).

## Requirements

- .NET Framework 4.7.2 Developer Pack (Visual Studio downloads it automatically
  if missing; when building from the command line it must be installed
  separately).
- No external library other than Accord.NET 3.8.0, already included in
  packages/ and referenced by the project.

## Running the program

Run the solution (F5 or Ctrl+F5). The behaviour depends on what is found in
bin/Debug:

- If a previously computed features.csv is present, the program asks:

      Found an existing 'features.csv' (... KB, written on ...).
        [U] use it      -> start training right away
        [R] rebuild it  -> extract the features again, about 10 minutes
      Your choice [U/R]:

  Typing U runs training and evaluation of the four models in a few seconds,
  with no need for the image dataset. Typing R (or having no such file)
  makes the program extract the features from the images again: see below
  for where to obtain them.

  Note: this prompt waits for keyboard input. If the program is launched from
  a script or from a non-interactive environment, execution will block at
  this point.

- If features.csv is not present, the program looks directly for the images
  (see below) and extracts the features: this takes about 10 minutes on
  ~18,800 images.

At the end of the run, the program prints "Done. Press any key to close..."
and waits for a key before closing the window: it is not stuck, this is
intentional (it helps whoever launches the executable by double-clicking
rather than from a terminal).

## Image dataset

If the image dataset is not already included in the submission, it must be
downloaded separately from Kaggle, "Tomato Leaf Disease":
https://www.kaggle.com/datasets/naveedgull/tomato-leaf-disease

It must be extracted as follows, relative to the project folder
(TomatoLeafClassification/):

    tomato_leaf_disease_dataset/
      Tomato Leaf Disease/
        train/   (one sub-folder per class)
        test/    (one sub-folder per class)

The program searches for this structure walking up to 8 parent directories
starting from the folder of the executable (bin/Debug or bin/Release), so the
exact position relative to the working directory is not critical: it is enough
that the tomato_leaf_disease_dataset folder is one of the ancestors. If it is
not found, the program prints the list of every path it checked.

## Reproducibility

Every run on the same features.csv produces identical results: both the random
generator of the project (train/test split and cross-validation folds, fixed
seed in Util/Constants.RandomSeed) and the internal generator of Accord.NET
(used by the BFGS optimiser of the logistic regression) are seeded at startup
with the same value.

## Project structure

See the report, Section 7 ("Software"), for the description of the classes, the
design patterns used and the choices concerning data structures and
computational complexity.