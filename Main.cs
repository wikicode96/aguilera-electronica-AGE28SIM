using Aguilera_connect;

bool test = false; // For testing

Aguilera aguilera = new Aguilera(423, "192.168.1.16", 1);

while (test != true)// For testing
{
    List<Zone> zones = aguilera.GetZonesStatus();
    aguilera.RepositionSystem(2);
    aguilera.SetPointsStatusForZones(2, 0, 13, 0);
    aguilera.SetPointsStatusForChannel(2, 1, 6, 0);
    List<Point> points = aguilera.GetPointsStatus(0, 1);
    test = true; // Just for testing
}

aguilera.Close();