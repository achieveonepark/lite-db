using System;

namespace Achieve.Database
{
    /// <summary>
    /// Data를 인게임에서 사용 할 Info로 Convert하여 테이블 데이터 값을 주입하는 확장 메소드
    /// </summary>
    public static class MappingExtensions
    {
        public static TInfo ToInfo<TInfo, TData>(this TData data)
            where TInfo : IMappableFrom<TData>, new()
        {
            var info = new TInfo();
            info.MapFrom(data);
            return info;
        }
        
        /// <summary>
        /// 아래 코드와 같이 사용합니다.
        /// </summary>
        /// <code>
        /// var towerInfo = towerData.ToInfo(data => new TowerInfo
        ///{
        ///    Id = data.Id, 
        ///    Attack = data.Attack, 
        ///    Count = 0, 
        ///    AttackInterval = data.AttackInterval, 
        ///    IsSplash = data.IsSplash, 
        ///    SplashRadius = data.SplashRadius, 
        ///    AttackType = data.AttackType, 
        ///    BulletId = data.BulletId
        ///});
        /// </code>
        /// <param name="data"></param>
        /// <param name="factory"></param>
        /// <typeparam name="TInfo"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <returns></returns>
        public static TInfo ToInfo<TInfo, TData>(
            this TData data,
            Func<TData, TInfo> factory) where TInfo : IMappableFrom<TData>
        {
            var info = factory(data);
            info.MapFrom(data);
            return info;
        }
    }
}