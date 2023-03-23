namespace Demo
{
    public enum ParticleSize
    {
        Low, 
        Medium, 
        High
        
        
    }

    public static class ParticleSizeUtility{
        public static float ToRadius(ParticleSize particleSize) => 
            particleSize switch{
                ParticleSize.Low => 0.1f,
                ParticleSize.Medium => 0.08f,
                ParticleSize.High => 0.06f,
                _ => 0.08f
            };
    }
}