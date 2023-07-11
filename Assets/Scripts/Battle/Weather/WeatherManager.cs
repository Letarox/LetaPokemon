using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager
{
    private Weather _currentWeather = new Weather();
    public Weather CurrentWeather => _currentWeather;
    public event Action<Weather> OnWeatherChange;
    public event Func<bool, IEnumerator> OnWeatherStartFinish;
    public event Func<BattleUnit, IEnumerator> OnWeatherDamage;
    public event Func<bool, IEnumerator> OnWeatherMove;

    public IEnumerator DisableWeather()
    {
        _currentWeather = WeatherDB.Weathers[WeatherID.None];
        OnWeatherChange?.Invoke(_currentWeather);
        yield return OnWeatherStartFinish?.Invoke(false);
    }
    public IEnumerator SetInitialWeather(Weather weather)
    {
        if (weather.Id != WeatherID.None)
        {
            _currentWeather = weather;
            _currentWeather.Duration = 1;
            _currentWeather.EnvironmentWeather = true;
            yield return OnWeatherStartFinish?.Invoke(true);
        }
    }
    public IEnumerator ChangeWeather(Weather weather)
    {
        if (_currentWeather != weather)
        {
            _currentWeather = weather;
            _currentWeather.Duration = 5;
            _currentWeather.EnvironmentWeather = false;
            yield return OnWeatherMove?.Invoke(true);
            OnWeatherChange?.Invoke(_currentWeather);
        }
        else
            yield return OnWeatherMove?.Invoke(false);
    }
    public IEnumerator WeatherMove(MoveEffects effects)
    {
        if (_currentWeather.Id != effects.WeatherEffect)
        {
            _currentWeather = WeatherDB.Weathers[effects.WeatherEffect];
            _currentWeather.Duration = 5;
            _currentWeather.EnvironmentWeather = false;
            yield return OnWeatherMove?.Invoke(true);
            OnWeatherChange?.Invoke(_currentWeather);
        }
        else
            yield return OnWeatherMove?.Invoke(false);
    }
    public IEnumerator WeatherAfterTurn(List<BattleUnit> units)
    {
        if (_currentWeather.Id != WeatherID.None)
        {
            //make sure that Environmental Weather lasts until replaced
            if (!_currentWeather.EnvironmentWeather)
                _currentWeather.Duration--;

            //remove weather or run its damage
            if (_currentWeather.Duration == 0)
            {                
                yield return DisableWeather();
            }
            else
            {
                if (_currentWeather?.OnAfterTurn != null)
                {
                    foreach (BattleUnit unit in units)
                        if (_currentWeather.OnAfterTurn.Invoke(unit.Pokemon))
                            yield return OnWeatherDamage?.Invoke(unit);
                }
            }
        }
    }
}
