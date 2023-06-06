namespace Aguilera_connect
{
    internal class Point
    {
        int numberInZone;
        int[] status = new int[2];
        /* 
         * { 0, 128 } = Alarma
         * { 0, 16 } = Avería
         * { 0, 8 } = Desconectado
         * { 0, 0 } = Reposo
         */
        int risk;
        /* 
         * El valor de la variable risk corresponde a la tabla de riesgos del manual.
         * Esta tabla se encuentra en el punto 6.1 del indice.
         * 
         * Resume de los valores mas frecuentes:
         * risk = 1 Detectores ópticos
         * risk = 4 Pulsadores
         * risk = 7 Salidas digitales
         */
        int CALA; // Un byte indicando la última causa que supuso un paso a alarma del punto. Si el punto no está en alarma será: 0xFF
        int CAVE; // Un byte indicando la última causa que supuso un paso a avería del punto. Si el punto no está ni ha estado en avería será: 0xFF.
        /* 
         * Los valores de CALA y CAVE creo que corresponden a la tabla de causas por riesgos del manual.
         * Y digo "creo" porque aún no ha sido verificado.
         * La tabla se encuentra en el punto 6.2 del indice
         */
        public Point(int PTO, int EST1, int EST2, int RIE, int CALA, int CAVE)
        {
            this.numberInZone = PTO;
            this.status[0] = EST1;
            this.status[1] = EST2;
            this.risk = RIE;
            this.CALA = CALA;
            this.CAVE = CAVE;
        }
    }
}
