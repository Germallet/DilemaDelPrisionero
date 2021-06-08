#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

static const float width = 1024;
static const float height = 786;
static const float dx = 1 / width;
static const float dy = 1 / height;

static const float T = 2.7;
static const float R = 1.2;
static const float C = 0.1;
static const float PointsMatrix[2][2] = { { R, 0 }, { T, C } };
static const float2 OffsetsMatrix[8] = { float2(-dx, dy),  float2(0, dy),  float2(dx, dy),
										 float2(-dx, 0),                   float2(dx, 0),
										 float2(-dx, -dy), float2(0, -dy), float2(dx, -dy) };

texture inputTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (inputTexture);
	MagFilter = None;
	MinFilter = None;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = input.Position;	
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}

float GetStrategy(float2 pos)
{
	return tex2D(textureSampler, pos).x;
}

float4 CalculatePointsPS(VertexShaderOutput input) : COLOR
{
	float selfStrategy = GetStrategy(input.TextureCoordinate);

	float totalPoints = 0;
	for (int i = 0; i < 8; i++)
		totalPoints += PointsMatrix[selfStrategy][GetStrategy(input.TextureCoordinate + OffsetsMatrix[i])];

	return float4(selfStrategy, 1/totalPoints, 0, 1);
}

float GetPoints(float2 pos)
{
	return 1/tex2D(textureSampler, pos).y;
}

float4 ChooseStrategyPS(VertexShaderOutput input) : COLOR
{
	float maxPoints = GetPoints(input.TextureCoordinate);
	float2 bestOffset = float2(0, 0);

	float candidatePoints;
	for (int i = 0; i < 8; i++)
	{
		candidatePoints = GetPoints(input.TextureCoordinate + OffsetsMatrix[i]);
		if (candidatePoints > maxPoints)
		{
			bestOffset = OffsetsMatrix[i];
			maxPoints = candidatePoints;
		}
	}

	return float4(GetStrategy(input.TextureCoordinate + bestOffset), 0, 0, 1);
}

technique CalculatePoints
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL CalculatePointsPS();
	}
};

technique ChooseStrategy
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL ChooseStrategyPS();
	}
};