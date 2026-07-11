using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.UI;

/// <summary>
/// v1.0.0 - Shows a small overlay map of the Spa-Francorchamps circuit
/// (VSR Kevin 2025 version) with a live marker (pin.png) that tracks the
/// player's position. The map only appears while the player is near the
/// actual circuit, using real telemetry points (embedded below) as
/// a reference line. The map position/size is configured via
/// SpaMinimap.ini.
/// </summary>
public class SpaMinimap : Script
{
    private CustomSprite mapSprite;
    private CustomSprite pinSprite;
    private bool enabled = true;

    // Screen position and size of the map overlay, loaded from SpaMinimap.ini
    // (uses these default values if the file or a value is missing/invalid).
    private PointF mapScreenPos = new PointF(20, 20);
    private SizeF mapScreenSize = new SizeF(260, 260);
    private SizeF pinSize = new SizeF(6, 6);

    // If the circuit mod moves in the GTA world (pure translation,
    // without rotation), put here how much it has moved and the map will compensate automatically.
    // Does not work if the circuit has also been rotated: in that case, the lap needs to be re-recorded.
    private float worldOffsetX = 0f;
    private float worldOffsetY = 0f;

    private const float CANVAS = 972f;

    // How close (in game units, approximately meters) the player needs to be
    // to the recorded line to be considered "on track". Adjust to your preference.
    private float ON_TRACK_DISTANCE = 20f;

    // --- World to pixel transformation of the spa.png canvas (972x972) ---
    // Derived from the bounding box of an actual recorded lap.
    private const float MIN_X = -6126.11f;
    private const float MIN_Y = 7120.5333333f;
    private const float SCALE = 0.4377140726f;
    private const float OFF_X = 208.3477504f;
    private const float OFF_Y = 40.0f;

    // Real telemetry from a recorded lap (~795 points, one every ~9m), used
    // only to detect if the player is near the circuit. Embedded
    // directly here to avoid needing any separate data file.
    private static readonly float[] TRACK_X = {
            -5788.69f, -5793.71f, -5798.72f, -5803.77f, -5808.86f, -5813.90f, -5818.98f, -5824.09f, -5829.25f, -5834.44f,
            -5839.62f, -5844.88f, -5850.22f, -5855.69f, -5861.07f, -5866.35f, -5871.64f, -5876.99f, -5882.44f, -5887.82f,
            -5892.87f, -5897.25f, -5901.00f, -5904.37f, -5907.69f, -5910.92f, -5913.57f, -5915.71f, -5917.71f, -5919.65f,
            -5921.66f, -5923.59f, -5925.44f, -5927.15f, -5928.35f, -5928.80f, -5928.52f, -5927.55f, -5926.07f, -5923.89f,
            -5920.97f, -5917.68f, -5913.92f, -5909.76f, -5905.37f, -5900.87f, -5896.38f, -5891.80f, -5887.05f, -5882.06f,
            -5876.76f, -5871.19f, -5865.32f, -5859.25f, -5853.05f, -5846.69f, -5840.17f, -5833.45f, -5826.62f, -5819.63f,
            -5812.48f, -5805.19f, -5797.71f, -5790.16f, -5782.53f, -5774.86f, -5767.12f, -5759.25f, -5751.34f, -5743.30f,
            -5735.16f, -5727.14f, -5719.02f, -5710.92f, -5702.93f, -5694.99f, -5687.11f, -5679.16f, -5671.29f, -5663.47f,
            -5655.54f, -5647.75f, -5640.06f, -5632.66f, -5625.33f, -5618.01f, -5610.69f, -5603.35f, -5596.18f, -5589.07f,
            -5581.97f, -5574.90f, -5567.76f, -5560.59f, -5553.33f, -5546.02f, -5538.70f, -5531.24f, -5523.84f, -5516.42f,
            -5509.04f, -5501.75f, -5494.42f, -5487.25f, -5480.23f, -5473.27f, -5466.44f, -5459.68f, -5453.12f, -5446.56f,
            -5440.01f, -5433.44f, -5426.71f, -5419.78f, -5412.63f, -5405.19f, -5397.47f, -5389.26f, -5380.98f, -5372.75f,
            -5364.79f, -5357.08f, -5349.77f, -5342.97f, -5336.61f, -5330.74f, -5325.45f, -5320.77f, -5316.63f, -5312.92f,
            -5309.49f, -5306.43f, -5303.63f, -5301.05f, -5298.87f, -5297.07f, -5295.50f, -5293.96f, -5292.08f, -5289.54f,
            -5286.43f, -5282.76f, -5278.43f, -5273.57f, -5268.63f, -5263.69f, -5258.56f, -5253.06f, -5247.30f, -5241.39f,
            -5235.44f, -5229.65f, -5224.04f, -5218.44f, -5212.74f, -5206.85f, -5200.88f, -5194.93f, -5189.18f, -5183.53f,
            -5177.78f, -5171.96f, -5166.19f, -5160.38f, -5154.57f, -5148.71f, -5142.85f, -5137.07f, -5131.43f, -5126.02f,
            -5120.66f, -5115.37f, -5110.34f, -5105.73f, -5101.53f, -5097.74f, -5094.22f, -5090.84f, -5087.50f, -5084.15f,
            -5080.81f, -5077.47f, -5074.14f, -5070.81f, -5067.49f, -5064.14f, -5060.77f, -5057.44f, -5054.08f, -5050.72f,
            -5047.35f, -5043.97f, -5040.60f, -5037.23f, -5033.87f, -5030.52f, -5027.14f, -5023.78f, -5020.42f, -5017.09f,
            -5013.77f, -5010.52f, -5007.33f, -5004.14f, -5000.86f, -4997.48f, -4994.04f, -4990.64f, -4987.37f, -4984.14f,
            -4980.91f, -4977.58f, -4974.16f, -4970.73f, -4967.29f, -4963.89f, -4960.49f, -4957.09f, -4953.70f, -4950.34f,
            -4947.04f, -4943.76f, -4940.47f, -4937.20f, -4933.93f, -4930.65f, -4927.34f, -4924.03f, -4920.70f, -4917.36f,
            -4914.01f, -4910.69f, -4907.34f, -4904.02f, -4900.64f, -4897.23f, -4893.91f, -4890.64f, -4887.37f, -4884.14f,
            -4881.01f, -4877.95f, -4874.96f, -4872.21f, -4869.69f, -4867.40f, -4865.18f, -4863.03f, -4860.99f, -4859.26f,
            -4858.15f, -4857.52f, -4857.46f, -4858.14f, -4859.62f, -4861.85f, -4864.85f, -4868.68f, -4873.17f, -4878.26f,
            -4883.49f, -4888.35f, -4892.78f, -4896.77f, -4900.44f, -4903.79f, -4906.68f, -4909.04f, -4910.85f, -4912.18f,
            -4912.71f, -4912.37f, -4911.34f, -4909.96f, -4908.43f, -4906.61f, -4904.46f, -4902.09f, -4899.81f, -4897.76f,
            -4895.89f, -4894.11f, -4892.41f, -4890.87f, -4889.39f, -4887.92f, -4886.54f, -4885.65f, -4885.65f, -4886.43f,
            -4887.90f, -4890.06f, -4892.96f, -4896.52f, -4900.82f, -4905.90f, -4911.69f, -4917.82f, -4923.92f, -4930.03f,
            -4936.23f, -4942.56f, -4949.01f, -4955.57f, -4962.31f, -4969.15f, -4976.11f, -4983.20f, -4990.32f, -4997.47f,
            -5004.70f, -5011.97f, -5019.29f, -5026.66f, -5034.13f, -5041.65f, -5049.21f, -5056.83f, -5064.52f, -5072.23f,
            -5080.02f, -5087.78f, -5095.40f, -5102.83f, -5110.00f, -5116.41f, -5122.17f, -5127.51f, -5132.61f, -5137.45f,
            -5142.38f, -5147.69f, -5153.42f, -5159.26f, -5165.29f, -5171.57f, -5177.85f, -5183.80f, -5189.33f, -5194.69f,
            -5199.88f, -5204.59f, -5208.73f, -5212.33f, -5215.35f, -5217.81f, -5219.68f, -5220.58f, -5220.43f, -5219.59f,
            -5218.02f, -5215.42f, -5211.90f, -5207.67f, -5202.70f, -5197.20f, -5191.42f, -5185.60f, -5179.72f, -5173.72f,
            -5167.52f, -5161.11f, -5154.52f, -5147.81f, -5141.08f, -5134.44f, -5127.79f, -5121.04f, -5114.28f, -5107.95f,
            -5102.10f, -5096.66f, -5091.93f, -5087.56f, -5083.61f, -5080.37f, -5078.00f, -5076.56f, -5076.06f, -5076.53f,
            -5077.86f, -5079.75f, -5081.94f, -5084.36f, -5087.17f, -5090.32f, -5093.64f, -5097.03f, -5100.47f, -5103.86f,
            -5107.33f, -5110.76f, -5114.07f, -5117.09f, -5119.85f, -5122.81f, -5125.99f, -5129.02f, -5131.92f, -5134.63f,
            -5137.11f, -5139.50f, -5141.88f, -5144.25f, -5146.60f, -5148.98f, -5151.37f, -5153.77f, -5156.18f, -5158.61f,
            -5161.02f, -5163.25f, -5165.36f, -5167.59f, -5169.97f, -5172.36f, -5174.79f, -5177.24f, -5179.67f, -5182.10f,
            -5184.48f, -5186.80f, -5189.06f, -5191.27f, -5193.44f, -5195.58f, -5197.93f, -5200.80f, -5204.31f, -5208.58f,
            -5213.49f, -5218.80f, -5224.52f, -5230.77f, -5237.51f, -5244.64f, -5252.04f, -5259.92f, -5268.13f, -5276.66f,
            -5285.39f, -5294.27f, -5303.25f, -5312.29f, -5321.47f, -5330.77f, -5340.11f, -5349.54f, -5358.97f, -5368.54f,
            -5377.99f, -5387.28f, -5396.40f, -5405.36f, -5414.23f, -5422.79f, -5431.07f, -5439.04f, -5446.54f, -5453.55f,
            -5460.04f, -5466.04f, -5471.71f, -5477.17f, -5482.20f, -5486.38f, -5489.95f, -5493.34f, -5496.66f, -5499.97f,
            -5503.29f, -5506.65f, -5510.13f, -5513.66f, -5517.25f, -5520.81f, -5524.37f, -5527.97f, -5531.52f, -5535.12f,
            -5538.70f, -5542.28f, -5545.88f, -5549.49f, -5553.09f, -5556.67f, -5560.27f, -5563.88f, -5567.48f, -5571.10f,
            -5574.66f, -5578.08f, -5581.11f, -5583.85f, -5586.66f, -5589.75f, -5593.27f, -5597.17f, -5601.30f, -5605.81f,
            -5610.83f, -5616.38f, -5622.44f, -5628.97f, -5635.91f, -5643.18f, -5650.67f, -5658.34f, -5666.13f, -5673.99f,
            -5681.81f, -5689.31f, -5696.54f, -5703.59f, -5710.59f, -5717.49f, -5724.46f, -5731.67f, -5739.19f, -5746.96f,
            -5754.86f, -5762.76f, -5770.63f, -5778.48f, -5786.17f, -5793.52f, -5800.57f, -5807.35f, -5813.62f, -5819.25f,
            -5824.38f, -5829.29f, -5834.22f, -5839.08f, -5843.86f, -5848.48f, -5852.92f, -5857.30f, -5861.81f, -5866.63f,
            -5871.77f, -5876.98f, -5882.29f, -5887.51f, -5892.74f, -5897.88f, -5902.72f, -5907.04f, -5910.82f, -5914.36f,
            -5918.01f, -5922.04f, -5926.57f, -5931.66f, -5937.26f, -5943.28f, -5949.64f, -5956.31f, -5963.19f, -5970.14f,
            -5976.98f, -5983.57f, -5989.96f, -5996.33f, -6002.62f, -6008.80f, -6014.97f, -6021.36f, -6028.00f, -6034.79f,
            -6041.79f, -6048.99f, -6056.15f, -6063.15f, -6070.19f, -6077.08f, -6083.62f, -6089.85f, -6095.87f, -6101.74f,
            -6107.19f, -6112.03f, -6116.33f, -6119.90f, -6122.65f, -6124.62f, -6125.81f, -6126.19f, -6125.51f, -6123.77f,
            -6121.44f, -6118.93f, -6116.30f, -6113.34f, -6110.04f, -6106.60f, -6103.15f, -6099.73f, -6096.42f, -6093.16f,
            -6089.52f, -6085.19f, -6080.19f, -6074.93f, -6069.61f, -6064.23f, -6058.82f, -6053.23f, -6047.35f, -6041.02f,
            -6034.40f, -6027.63f, -6020.80f, -6013.95f, -6007.07f, -6000.06f, -5992.77f, -5985.33f, -5977.84f, -5970.35f,
            -5962.80f, -5955.12f, -5947.22f, -5939.13f, -5930.84f, -5922.28f, -5913.57f, -5904.63f, -5895.46f, -5886.23f,
            -5876.96f, -5867.61f, -5858.20f, -5848.80f, -5839.42f, -5830.06f, -5820.68f, -5811.27f, -5801.75f, -5792.26f,
            -5782.79f, -5773.29f, -5763.82f, -5754.42f, -5745.22f, -5736.18f, -5727.23f, -5718.37f, -5709.72f, -5701.40f,
            -5693.48f, -5685.78f, -5678.18f, -5670.77f, -5663.55f, -5656.62f, -5650.00f, -5643.76f, -5637.91f, -5632.36f,
            -5627.15f, -5622.27f, -5617.60f, -5613.03f, -5608.62f, -5604.53f, -5600.68f, -5596.86f, -5592.94f, -5588.96f,
            -5584.98f, -5581.04f, -5577.17f, -5573.14f, -5568.97f, -5564.86f, -5561.05f, -5557.95f, -5555.76f, -5554.35f,
            -5553.58f, -5553.58f, -5554.33f, -5555.70f, -5557.61f, -5560.35f, -5564.00f, -5568.05f, -5572.02f, -5575.90f,
            -5579.84f, -5583.83f, -5587.82f, -5591.85f, -5595.88f, -5599.96f, -5604.06f, -5608.18f, -5612.32f, -5616.46f,
            -5620.59f, -5624.74f, -5628.85f, -5632.98f, -5637.11f, -5641.23f, -5645.34f, -5649.27f, -5652.63f, -5655.33f,
            -5657.59f, -5659.72f, -5661.99f, -5664.50f, -5666.85f, -5669.01f, -5671.05f, -5672.73f, -5674.11f, -5675.54f,
            -5677.04f, -5678.45f, -5679.73f, -5680.95f, -5682.12f, -5683.28f, -5684.44f, -5685.57f, -5686.59f, -5687.48f,
            -5688.30f, -5689.03f, -5689.64f, -5690.15f, -5690.63f, -5691.10f, -5691.49f, -5691.49f, -5690.82f, -5689.34f,
            -5687.03f, -5683.99f, -5680.37f, -5676.25f, -5671.76f, -5667.33f, -5663.11f, -5658.64f, -5653.86f, -5649.08f,
            -5644.63f, -5640.68f, -5637.22f, -5634.43f, -5632.53f, -5631.55f, -5631.57f, -5632.73f, -5634.89f, -5637.69f,
            -5640.81f, -5644.11f, -5647.48f, -5650.87f, -5654.24f, -5657.62f, -5661.08f, -5664.72f, -5668.55f, -5672.51f,
            -5676.56f, -5680.65f, -5684.82f, -5689.05f, -5693.36f, -5697.73f, -5702.16f, -5706.66f, -5711.21f, -5715.81f,
            -5720.46f, -5725.13f, -5729.84f, -5734.57f, -5739.37f, -5744.18f, -5749.05f, -5753.95f, -5758.78f, -5763.47f,
            -5768.26f, -5773.33f, -5778.55f, -5783.68f, -5788.69f
    };

    private static readonly float[] TRACK_Y = {
            8914.38f, 8922.46f, 8930.50f, 8938.59f, 8946.76f, 8954.88f, 8963.06f, 8971.29f, 8979.60f, 8987.97f,
            8996.31f, 9004.71f, 9013.06f, 9021.44f, 9029.89f, 9038.48f, 9047.09f, 9055.67f, 9064.27f, 9072.68f,
            9080.56f, 9087.40f, 9093.27f, 9098.56f, 9103.78f, 9108.89f, 9113.11f, 9116.54f, 9119.76f, 9122.90f,
            9126.19f, 9129.36f, 9132.40f, 9135.34f, 9138.28f, 9141.17f, 9144.00f, 9146.95f, 9150.07f, 9153.06f,
            9155.62f, 9157.27f, 9158.17f, 9158.45f, 9158.00f, 9156.65f, 9154.53f, 9151.97f, 9149.27f, 9146.54f,
            9143.89f, 9141.38f, 9138.88f, 9136.30f, 9133.56f, 9130.66f, 9127.67f, 9124.66f, 9121.65f, 9118.57f,
            9115.42f, 9112.19f, 9108.86f, 9105.44f, 9101.75f, 9097.87f, 9093.90f, 9089.84f, 9085.73f, 9081.54f,
            9077.07f, 9072.41f, 9067.60f, 9062.64f, 9057.45f, 9051.97f, 9046.29f, 9040.44f, 9034.45f, 9028.36f,
            9022.12f, 9015.82f, 9009.16f, 9002.16f, 8994.92f, 8987.60f, 8980.21f, 8972.71f, 8965.16f, 8957.45f,
            8949.60f, 8941.75f, 8933.86f, 8925.99f, 8918.06f, 8910.11f, 8902.25f, 8894.35f, 8886.52f, 8878.60f,
            8870.58f, 8862.57f, 8854.48f, 8846.58f, 8838.84f, 8831.18f, 8823.66f, 8816.23f, 8809.02f, 8801.82f,
            8794.63f, 8787.58f, 8780.86f, 8774.74f, 8768.97f, 8763.27f, 8757.98f, 8753.02f, 8748.02f, 8742.77f,
            8737.58f, 8732.52f, 8727.45f, 8722.14f, 8716.60f, 8710.61f, 8704.03f, 8697.07f, 8689.78f, 8682.12f,
            8674.27f, 8666.22f, 8658.03f, 8649.69f, 8641.16f, 8632.55f, 8623.71f, 8614.80f, 8605.89f, 8597.11f,
            8588.47f, 8580.03f, 8571.90f, 8563.99f, 8556.12f, 8548.02f, 8539.84f, 8531.73f, 8523.80f, 8515.91f,
            8507.96f, 8499.89f, 8491.67f, 8483.40f, 8475.21f, 8467.10f, 8459.01f, 8450.91f, 8442.63f, 8434.24f,
            8425.85f, 8417.47f, 8409.16f, 8400.79f, 8392.41f, 8383.96f, 8375.49f, 8367.02f, 8358.33f, 8349.52f,
            8340.60f, 8331.63f, 8322.52f, 8313.19f, 8303.70f, 8294.02f, 8284.17f, 8274.39f, 8264.59f, 8254.72f,
            8244.83f, 8234.92f, 8225.02f, 8215.14f, 8205.23f, 8195.29f, 8185.27f, 8175.39f, 8165.43f, 8155.48f,
            8145.50f, 8135.48f, 8125.51f, 8115.53f, 8105.55f, 8095.62f, 8085.59f, 8075.63f, 8065.62f, 8055.62f,
            8045.58f, 8035.45f, 8025.35f, 8015.21f, 8005.11f, 7995.08f, 7985.03f, 7974.97f, 7964.93f, 7954.83f,
            7944.74f, 7934.72f, 7924.66f, 7914.66f, 7904.59f, 7894.50f, 7884.33f, 7874.16f, 7863.99f, 7853.82f,
            7843.68f, 7833.58f, 7823.47f, 7813.38f, 7803.31f, 7793.17f, 7783.04f, 7772.87f, 7762.68f, 7752.49f,
            7742.24f, 7732.11f, 7721.86f, 7711.75f, 7701.62f, 7691.51f, 7681.38f, 7671.19f, 7661.10f, 7651.17f,
            7641.52f, 7632.07f, 7622.84f, 7614.30f, 7606.45f, 7599.27f, 7592.29f, 7585.48f, 7578.91f, 7572.45f,
            7566.09f, 7559.85f, 7553.51f, 7547.19f, 7541.14f, 7535.34f, 7529.92f, 7524.86f, 7520.35f, 7516.31f,
            7512.73f, 7509.59f, 7506.44f, 7503.09f, 7499.24f, 7494.86f, 7489.96f, 7484.57f, 7478.88f, 7472.81f,
            7466.49f, 7460.01f, 7453.40f, 7446.75f, 7439.94f, 7433.01f, 7426.08f, 7418.96f, 7411.85f, 7404.70f,
            7397.69f, 7390.98f, 7384.60f, 7378.82f, 7373.23f, 7367.65f, 7361.71f, 7355.30f, 7348.89f, 7342.43f,
            7335.94f, 7329.52f, 7323.17f, 7317.14f, 7311.42f, 7306.35f, 7301.93f, 7297.68f, 7293.25f, 7288.62f,
            7283.86f, 7279.02f, 7274.08f, 7269.13f, 7264.18f, 7259.22f, 7254.19f, 7249.08f, 7243.89f, 7238.63f,
            7233.25f, 7227.74f, 7222.14f, 7216.50f, 7210.80f, 7205.07f, 7199.31f, 7193.51f, 7187.65f, 7181.76f,
            7175.79f, 7169.80f, 7163.91f, 7158.15f, 7152.60f, 7147.64f, 7143.20f, 7139.10f, 7135.30f, 7132.03f,
            7129.02f, 7126.36f, 7124.12f, 7122.46f, 7121.24f, 7120.53f, 7120.62f, 7121.65f, 7123.60f, 7126.25f,
            7129.54f, 7133.57f, 7138.33f, 7143.73f, 7149.64f, 7155.75f, 7162.06f, 7168.44f, 7174.80f, 7181.19f,
            7187.60f, 7193.83f, 7199.49f, 7204.39f, 7208.59f, 7212.28f, 7215.89f, 7219.69f, 7223.71f, 7227.86f,
            7232.00f, 7236.12f, 7240.31f, 7244.68f, 7249.18f, 7253.67f, 7258.17f, 7262.76f, 7267.37f, 7271.69f,
            7275.81f, 7280.10f, 7284.30f, 7288.84f, 7293.94f, 7299.69f, 7305.92f, 7312.56f, 7319.44f, 7326.42f,
            7333.42f, 7340.50f, 7347.80f, 7355.24f, 7362.72f, 7370.29f, 7378.02f, 7385.86f, 7393.90f, 7402.07f,
            7410.49f, 7419.05f, 7427.84f, 7436.76f, 7445.80f, 7454.87f, 7463.94f, 7473.14f, 7482.54f, 7491.96f,
            7501.55f, 7511.25f, 7521.09f, 7531.01f, 7540.90f, 7550.90f, 7560.97f, 7571.10f, 7581.26f, 7591.49f,
            7601.75f, 7612.05f, 7622.48f, 7632.90f, 7643.38f, 7653.78f, 7664.30f, 7674.87f, 7685.42f, 7696.08f,
            7706.71f, 7717.13f, 7727.37f, 7737.36f, 7747.15f, 7756.74f, 7766.05f, 7775.05f, 7783.50f, 7791.27f,
            7798.47f, 7805.48f, 7812.02f, 7817.62f, 7822.38f, 7826.76f, 7830.88f, 7834.44f, 7837.08f, 7838.93f,
            7840.28f, 7841.40f, 7842.22f, 7842.76f, 7843.10f, 7843.24f, 7843.05f, 7842.47f, 7841.60f, 7840.36f,
            7838.54f, 7835.83f, 7832.38f, 7828.44f, 7823.94f, 7818.95f, 7813.36f, 7807.24f, 7800.53f, 7793.14f,
            7785.38f, 7777.17f, 7768.55f, 7759.83f, 7750.76f, 7741.36f, 7731.70f, 7721.85f, 7712.00f, 7702.07f,
            7692.13f, 7682.18f, 7672.16f, 7662.22f, 7652.16f, 7642.18f, 7632.22f, 7622.13f, 7612.15f, 7602.07f,
            7592.02f, 7581.95f, 7571.85f, 7561.76f, 7551.67f, 7541.64f, 7531.56f, 7521.48f, 7511.42f, 7501.31f,
            7491.37f, 7481.86f, 7473.45f, 7465.90f, 7458.61f, 7451.67f, 7445.22f, 7439.01f, 7432.88f, 7427.24f,
            7422.26f, 7417.72f, 7413.64f, 7410.20f, 7407.68f, 7405.90f, 7404.55f, 7403.86f, 7403.96f, 7404.83f,
            7406.62f, 7409.39f, 7413.07f, 7417.30f, 7421.79f, 7426.06f, 7429.54f, 7432.10f, 7433.90f, 7434.96f,
            7435.14f, 7434.37f, 7432.80f, 7430.55f, 7427.53f, 7423.59f, 7418.99f, 7413.70f, 7407.58f, 7400.61f,
            7393.10f, 7385.38f, 7377.45f, 7369.42f, 7361.32f, 7353.02f, 7344.59f, 7336.09f, 7327.39f, 7318.67f,
            7310.00f, 7301.48f, 7292.82f, 7284.15f, 7275.32f, 7266.60f, 7258.39f, 7251.07f, 7244.71f, 7238.80f,
            7233.13f, 7228.05f, 7223.68f, 7219.91f, 7216.62f, 7214.00f, 7212.13f, 7210.96f, 7210.66f, 7211.37f,
            7213.11f, 7215.87f, 7219.31f, 7223.14f, 7227.23f, 7231.74f, 7236.52f, 7241.26f, 7245.92f, 7250.58f,
            7255.15f, 7259.62f, 7263.99f, 7268.34f, 7272.84f, 7277.46f, 7282.29f, 7287.66f, 7293.41f, 7299.46f,
            7306.11f, 7313.43f, 7321.21f, 7329.42f, 7337.95f, 7346.76f, 7355.70f, 7364.75f, 7373.82f, 7382.82f,
            7391.81f, 7400.73f, 7409.72f, 7418.63f, 7427.46f, 7436.30f, 7445.11f, 7453.98f, 7462.92f, 7471.90f,
            7480.80f, 7489.43f, 7497.94f, 7506.44f, 7514.95f, 7523.49f, 7532.05f, 7540.65f, 7549.06f, 7557.22f,
            7565.25f, 7573.28f, 7581.28f, 7589.28f, 7597.27f, 7605.16f, 7612.84f, 7620.41f, 7628.00f, 7635.56f,
            7643.13f, 7650.58f, 7657.77f, 7664.76f, 7671.47f, 7677.85f, 7684.04f, 7689.86f, 7695.33f, 7700.66f,
            7705.96f, 7711.32f, 7716.75f, 7722.19f, 7727.63f, 7733.07f, 7738.51f, 7743.84f, 7749.05f, 7754.19f,
            7759.33f, 7764.50f, 7769.71f, 7775.03f, 7780.64f, 7786.49f, 7792.41f, 7798.53f, 7804.93f, 7811.74f,
            7818.99f, 7826.47f, 7834.09f, 7841.87f, 7849.86f, 7858.06f, 7866.52f, 7875.26f, 7884.27f, 7893.52f,
            7902.95f, 7912.61f, 7922.28f, 7932.00f, 7941.89f, 7952.00f, 7962.24f, 7972.51f, 7982.74f, 7992.93f,
            8003.06f, 8013.08f, 8022.92f, 8032.71f, 8042.48f, 8052.24f, 8062.15f, 8072.22f, 8082.48f, 8092.88f,
            8103.34f, 8113.84f, 8124.23f, 8134.58f, 8144.85f, 8154.90f, 8164.63f, 8174.15f, 8183.77f, 8193.44f,
            8203.11f, 8212.81f, 8222.47f, 8232.20f, 8241.92f, 8251.66f, 8261.39f, 8271.13f, 8280.87f, 8290.64f,
            8300.42f, 8310.24f, 8319.98f, 8329.79f, 8339.59f, 8349.40f, 8359.26f, 8369.14f, 8379.20f, 8389.47f,
            8399.84f, 8410.26f, 8420.60f, 8430.92f, 8441.24f, 8451.62f, 8462.08f, 8472.52f, 8483.03f, 8493.58f,
            8504.10f, 8514.66f, 8525.19f, 8535.77f, 8546.31f, 8556.87f, 8567.43f, 8577.82f, 8587.14f, 8595.33f,
            8602.38f, 8608.26f, 8613.02f, 8617.01f, 8620.83f, 8624.65f, 8628.48f, 8632.13f, 8635.48f, 8638.37f,
            8640.90f, 8643.02f, 8644.75f, 8645.86f, 8646.19f, 8645.74f, 8644.88f, 8643.96f, 8643.38f, 8643.38f,
            8644.14f, 8645.74f, 8648.30f, 8651.68f, 8655.62f, 8660.06f, 8664.86f, 8669.71f, 8674.45f, 8679.05f,
            8683.66f, 8688.42f, 8693.42f, 8698.64f, 8704.13f, 8709.85f, 8715.72f, 8721.66f, 8727.71f, 8733.86f,
            8740.10f, 8746.40f, 8752.79f, 8759.30f, 8765.90f, 8772.61f, 8779.40f, 8786.30f, 8793.27f, 8800.35f,
            8807.52f, 8814.77f, 8822.12f, 8829.50f, 8836.97f, 8844.48f, 8852.09f, 8859.77f, 8867.52f, 8875.44f,
            8883.35f, 8891.13f, 8898.89f, 8906.72f, 8914.38f
    };

    private readonly string scriptDir;
    private readonly Dictionary<string, Dictionary<string, string>> ini = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

    public SpaMinimap()
    {
        // Scripts compiled on-the-fly by SHVDN from loose .cs files don't have
        // an actual assembly file on disk, so Assembly.Location is empty. Instead,
        // locate the game's "scripts" folder from the running executable path.
        scriptDir = Path.Combine(Application.StartupPath, "scripts");

        LoadIni();
        ApplyIniValues();

        mapSprite = new CustomSprite(Path.Combine(scriptDir, "SpaMinimap/spa.png"), mapScreenSize, mapScreenPos);
        pinSprite = new CustomSprite(Path.Combine(scriptDir, "SpaMinimap/pin.png"), pinSize, mapScreenPos);

        Tick += OnTick;
    }

    /// <summary>
    /// Tiny INI reader: sections in [Brackets], key=value lines,
    /// ";" or "#" for comments. No external dependencies required.
    /// </summary>
    private void LoadIni()
    {
        string path = Path.Combine(scriptDir, "SpaMinimap.ini");
        if (!File.Exists(path))
        {
            Notification.PostTicker("~y~SpaMinimap.ini not found, using default values.", false);
            return;
        }

        string currentSection = "";
        foreach (var raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentSection = line.Substring(1, line.Length - 2).Trim();
                if (!ini.ContainsKey(currentSection))
                    ini[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            int eq = line.IndexOf('=');
            if (eq < 0) continue;

            string key = line.Substring(0, eq).Trim();
            string value = line.Substring(eq + 1).Trim();

            if (!ini.ContainsKey(currentSection))
                ini[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            ini[currentSection][key] = value;
        }
    }

    private float GetIniFloat(string section, string key, float defaultValue)
    {
        Dictionary<string, string> kv;
        string raw;
        if (ini.TryGetValue(section, out kv) && kv.TryGetValue(key, out raw))
        {
            float parsed;
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            Notification.PostTicker("~r~Invalid value in SpaMinimap.ini [" + section + "] " + key + "=" + raw + ", using " + defaultValue.ToString(CultureInfo.InvariantCulture), false);
        }
        return defaultValue;
    }

    private void ApplyIniValues()
    {
        float posX = GetIniFloat("Map", "PosX", mapScreenPos.X);
        float posY = GetIniFloat("Map", "PosY", mapScreenPos.Y);
        float w = GetIniFloat("Map", "Width", mapScreenSize.Width);
        float h = GetIniFloat("Map", "Height", mapScreenSize.Height);
        float pinW = GetIniFloat("Pin", "Width", pinSize.Width);
        float pinH = GetIniFloat("Pin", "Height", pinSize.Height);
        worldOffsetX = GetIniFloat("Circuit", "OffsetX", worldOffsetX);
        worldOffsetY = GetIniFloat("Circuit", "OffsetY", worldOffsetY);
        ON_TRACK_DISTANCE = GetIniFloat("Circuit", "OnTrackDistance", ON_TRACK_DISTANCE);

        mapScreenPos = new PointF(posX, posY);
        mapScreenSize = new SizeF(Math.Max(1f, w), Math.Max(1f, h));
        pinSize = new SizeF(Math.Max(1f, pinW), Math.Max(1f, pinH));
    }

    private bool IsNearTrack(float x, float y)
    {
        float best = float.MaxValue;
        int count = TRACK_X.Length;
        for (int i = 0; i < count - 1; i++)
        {
            float d = DistancePointToSegment(x, y, TRACK_X[i], TRACK_Y[i], TRACK_X[i + 1], TRACK_Y[i + 1]);
            if (d < best) best = d;
            if (best <= ON_TRACK_DISTANCE) return true; // early exit
        }
        return best <= ON_TRACK_DISTANCE;
    }

    private static float DistancePointToSegment(float px, float py, float ax, float ay, float bx, float by)
    {
        float abx = bx - ax, aby = by - ay;
        float apx = px - ax, apy = py - ay;
        float lenSq = abx * abx + aby * aby;
        float t = lenSq > 0.0001f ? (apx * abx + apy * aby) / lenSq : 0f;
        t = Math.Max(0f, Math.Min(1f, t));
        float cx = ax + abx * t;
        float cy = ay + aby * t;
        float dx = px - cx, dy = py - cy;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!enabled)
            return;

        var rawPos = Game.Player.Character.Position;
        float wx = rawPos.X - worldOffsetX;
        float wy = rawPos.Y - worldOffsetY;

        if (!IsNearTrack(wx, wy))
            return;

        mapSprite.Draw();

        // World to pixel space of the 972x972 canvas, north pointing up, no rotation.
        float px = OFF_X + (wx - MIN_X) * SCALE;
        float py = CANVAS - OFF_Y - (wy - MIN_Y) * SCALE;

        // Scale from the original 972x972 canvas to the map size on screen.
        float sx = mapScreenPos.X + (px / CANVAS) * mapScreenSize.Width;
        float sy = mapScreenPos.Y + (py / CANVAS) * mapScreenSize.Height;

        pinSprite.Position = new PointF(sx - pinSize.Width / 2f, sy - pinSize.Height / 2f);
        pinSprite.Draw();
    }
}
