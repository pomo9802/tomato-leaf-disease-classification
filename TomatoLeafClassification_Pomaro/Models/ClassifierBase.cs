using System;
using TomatoLeafClassification.Util;

namespace TomatoLeafClassification.Models
{
    public class ClassifierBase : IClassifier
    {
        protected bool Trained;

        /// <summary>
        /// Static constructor. Sets the random seed for reproducibility.
        /// </summary>
        static ClassifierBase()
        {
            Accord.Math.Random.Generator.Seed = Constants.RandomSeed;
        }

        /// <summary>
        /// Protected constructor: the class is meant to be used only as a base for concrete
        /// classifiers, not to be instantiated directly.
        /// </summary>
        protected ClassifierBase()
        { }

        /// <summary>
        /// Name of the model. By default the class name; concrete classifiers override it to
        /// include their hyperparameters.
        /// </summary>
        public virtual string Name
        {
            get { return GetType().Name; }
        }

        /// <summary>
        /// Train the model on labelled data.
        /// </summary>
        /// <exception cref="NotSupportedException">if the model does not implement training</exception>
        public virtual void Train(double[][] inputs, int[] outputs)
        {
            throw new NotSupportedException(Name + " does not implement Train.");
        }

        /// <summary>
        /// Predict the class of a single sample
        /// </summary>
        /// <exception cref="NotSupportedException">if the model does not implement prediction</exception>
        public virtual int Predict(double[] input)
        {
            throw new NotSupportedException(Name + " does not implement Predict.");
        }

        /// <summary>
        /// Create an untrained copy of the classifier.
        /// </summary>
        /// <exception cref="NotSupportedException">if the model does not implement this operation</exception>
        public virtual IClassifier CreateNew()
        {
            throw new NotSupportedException(Name + " does not implement CreateNew.");
        }

        /// <summary>
        /// Default implementation of the multi-sample prediction. Calls the single sample
        /// prediction for each sample.
        /// </summary>
        public virtual int[] Predict(double[][] inputs)
        {
            EnsureTrained();
            int[] result = new int[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                result[i] = Predict(inputs[i]);
            return result;
        }


        /// <summary>
        /// Override of toString. Canonical text form of a model (name and if trained or not).
        /// </summary>
        public override string ToString()
        {
            if (Trained) return Name + " [trained]";
            else return Name + " [not trained]";
        }


        /// <summary>
        /// Raise an exception if the model is used for prediction before being trained.
        /// </summary>
        /// <exception cref="InvalidOperationException">if the model is not trained</exception>
        protected void EnsureTrained()
        {
            if (!Trained)
                throw new InvalidOperationException(Name + " is not trained.");
        }
    }
}
