﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Encog.Neural.Networks.Synapse;
using Encog.Neural.NeuralData;
using Encog.Matrix;
using Encog.Neural.Networks.Layers;
using log4net.Repository.Hierarchy;
using Encog.Neural.Data;
using log4net;

namespace Encog.Neural.Networks.Training.Hopfield
{
    /// <summary>
    /// This class is used to train a Hopfield neural network. A hopfield neural
    /// network can be created by using the basic layer and connecting it to itself,
    /// forming a single layer recurrent neural network.
    /// 
    /// This is an unsupervised training algorithm.  Ideal values should not
    /// be specified in the training set.  If ideal values are present, they
    /// will be ignored.
    /// </summary>
    public class TrainHopfield : BasicTraining
    {

        /// <summary>
        /// The network being trained.
        /// </summary>
        private BasicNetwork network;

        /// <summary>
        /// The logging object.
        /// </summary>
        private readonly ILog logger = LogManager.GetLogger(typeof(BasicNetwork));

        /// <summary>
        /// Construct a Hopfield training class.
        /// </summary>
        /// <param name="trainingSet">The training set to use.</param>
        /// <param name="network">The network to train.</param>
        public TrainHopfield(INeuralDataSet trainingSet,
                 BasicNetwork network)
        {
            this.network = network;
            this.Training = trainingSet;
            this.Error = 0;
        }

        /**
         * Update the Hopfield weights after training.
         * @param target The target synapse.
         * @param delta The amoun to change the weights by.
         */
        private void ConvertHopfieldMatrix(ISynapse target,
                 Matrix.Matrix delta)
        {
            // add the new weight matrix to what is there already
            for (int row = 0; row < delta.Rows; row++)
            {
                for (int col = 0; col < delta.Rows; col++)
                {
                    target.WeightMatrix[row, col] = delta[row, col];
                }
            }
        }

        /// <summary>
        /// The network being trained.
        /// </summary>
        public override BasicNetwork Network
        {
            get
            {
                return this.network;
            }
        }

        /// <summary>
        /// Perform one training iteration.
        /// </summary>
        public override void Iteration()
        {

            if (this.logger.IsInfoEnabled)
            {
                this.logger.Info("Performing Hopfield iteration.");
            }

            PreIteration();

            foreach (ILayer layer in this.network.Structure.Layers)
            {
                foreach (ISynapse synapse in layer.Next)
                {
                    if (synapse.IsSelfConnected)
                    {
                        TrainHopfieldSynapse(synapse);
                    }
                }
            }

            PostIteration();
        }

        /// <summary>
        /// Once the hopfield synapse has been found, this method is called
        /// to train it.
        /// </summary>
        /// <param name="recurrent">The hopfield layer.</param>
        private void TrainHopfieldSynapse(ISynapse recurrent)
        {
            foreach (INeuralDataPair data in this.Training)
            {
                TrainHopfieldSynapse(recurrent, data.Input);
            }
        }

        /// <summary>
        /// Train the neural network for the specified pattern. The neural network
        /// can be trained for more than one pattern. To do this simply call the
        /// train method more than once.
        /// </summary>
        /// <param name="synapse">The synapse to train.</param>
        /// <param name="pattern">The pattern to train for.</param>
        public void TrainHopfieldSynapse(ISynapse synapse,
                 INeuralData pattern)
        {

            // Create a row matrix from the input, convert boolean to bipolar
            Matrix.Matrix m2 = Matrix.Matrix.CreateRowMatrix(pattern.Data);
            // Transpose the matrix and multiply by the original input matrix
            Matrix.Matrix m1 = MatrixMath.Transpose(m2);
            Matrix.Matrix m3 = MatrixMath.Multiply(m1, m2);
            // matrix 3 should be square by now, so create an identity
            // matrix of the same size.
            Matrix.Matrix identity = MatrixMath.Identity(m3.Rows);

            // subtract the identity matrix
            Matrix.Matrix m4 = MatrixMath.Subtract(m3, identity);

            // now add the calculated matrix, for this pattern, to the
            // existing weight matrix.
            ConvertHopfieldMatrix(synapse, m4);
        }
    }

}