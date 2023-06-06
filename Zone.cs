namespace Aguilera_connect
{
    internal class Zone
    {
        int[] zoneNumber = new int[2];
        bool alarm;
        bool preAlarm;
        bool activate;
        bool fault;
        bool desconect;
        bool testing;
        int totalPoints;
        int alarmPoints;
        int preAlarmPoints;
        int activatePoints;
        int faultPoints;
        int desconectPoints;
        int testingPoints;
        public Zone(int byteZone1,  int byteZone2, int totalPoints, int preAlarmPoints, int alarmPoints, int activatePoints, int faultPoints, int desconectPoints, int testingPoints)
        {
            this.zoneNumber[0] = byteZone1;
            this.zoneNumber[1] = byteZone2;
            this.totalPoints = totalPoints;
            this.preAlarmPoints = preAlarmPoints; 
            this.alarmPoints = alarmPoints; 
            this.activatePoints = activatePoints;
            this.faultPoints = faultPoints;
            this.desconectPoints = desconectPoints;
            this.testingPoints = testingPoints;

            if (this.preAlarmPoints > 0) preAlarm = true;
            if (this.alarmPoints > 0) alarm = true;
            if (this.activatePoints > 0) activate = true;
            if (this.faultPoints > 0) fault = true;
            if (this.desconectPoints > 0) desconect = true;
            if (this.testingPoints > 0) testing = true;
        }
    }
}
