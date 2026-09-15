using NLDMAP.Domain.ValueObjects;

namespace NLDMAP.Domain.Entities;

    public class Report
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        //Relacion con el ciudadano que reporta
        public Guid MissingPersonId { get; private set; }
        public Guid UserId { get; private set; }
        public  string MissingPersonRelation { get; private set; } //Madre, amigo/conocido etc.

        //informacion de la desaparicion
        public  DisappearanceEvents Events { get; private set; }

        //Datos de consentimiento legal
        public bool ConsentUseExclusive { get; private set; }
        public bool RequestInformationPublic { get; private set; }

        //estado del reporte
        public  ReportStatus Status { get; private set; } = ReportStatus.Pendiente;
        
        //Constructor vacio requerido por EF
        protected Report(){}
        
        //Constructor de dominio
        public  Report (
            Guid missingPersonId,
            Guid userId,
            string missingPersonRelation,
            DisappearanceEvents events,
            bool consentUseExclusive,
            bool requestInformationPublic)
        {
            //Validaciones basicas
            if (missingPersonId == Guid.Empty) throw new ArgumentException("ID de persona invalido");
            if(userId == Guid.Empty) throw new ArgumentException("ID de usuario invalido");
            if (events == null) throw new ArgumentNullException(nameof(events));
            
            //Asignaciones
            MissingPersonId = missingPersonId;
            UserId = userId;
            MissingPersonRelation = missingPersonRelation;
            Events = events;
            ConsentUseExclusive = consentUseExclusive;
            RequestInformationPublic = requestInformationPublic;
        }
        
        //Metodos para cambiar el estado 
        public void MarkAsActive() => Status = ReportStatus.Activo;
        public void MarkAsInvalid() => Status = ReportStatus.SinValidar;

    }

    public enum ReportStatus
    {
        Pendiente,
        SinValidar,
        Activo
    }
    
